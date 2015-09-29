using System;
using Xamarin.Forms;
using NavigationExample;
using NavigationExample.iOS;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;
using CoreGraphics;

[assembly: ExportRenderer (typeof (CustomNavigation), typeof (CustomNavigationRenderer))]
namespace NavigationExample.iOS
{
	public class CustomNavigationRenderer:NavigationRenderer, IUIGestureRecognizerDelegate
	{
		UIView blurView;
		bool interactive=false;

		protected override void OnElementChanged (VisualElementChangedEventArgs e)
		{
			base.OnElementChanged (e);
			if (e == null || e.NewElement == null)
				return;
			ModalPresentationStyle = UIModalPresentationStyle.Custom;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			//We need to override "ShouldBegin" so that we have swipe back when Navigation bar is not shown
			InteractivePopGestureRecognizer.Delegate = this;
			InteractivePopGestureRecognizer.AddTarget (HandleOnstagePan);
			Delegate = new SlideLeftRightNavigationDelegate ();
		}

		//We need to override "ShouldBegin" so that we have swipe back when Navigation bar is not shown
		[Export("gestureRecognizerShouldBegin:")]
		public bool ShouldBegin(UIGestureRecognizer recognizer) {
			if (recognizer is UIScreenEdgePanGestureRecognizer && 
				ViewControllers.Length == 1) {
				return false;
			}
			return true;
		}

		public void HandleOnstagePan(NSObject obj){
			UIScreenEdgePanGestureRecognizer pan = (UIScreenEdgePanGestureRecognizer)obj;

			// how much distance have we panned in reference to the parent view?
			var translation = pan.TranslationInView(pan.View);

			// do some math to translate this to a percentage based value
			nfloat d =  (nfloat)(translation.X / pan.View.Bounds.Width);

			// now lets deal with different states that the gesture recognizer sends
			switch (pan.State) {
			case UIGestureRecognizerState.Changed:
				// update progress of the transition 
				SlideLeftRightNavigationDelegate.transition.UpdateInteractiveTransition (d);
				break;
			case UIGestureRecognizerState.Ended:
			case UIGestureRecognizerState.Cancelled:
				SlideLeftRightNavigationDelegate.transition.FinishInteractiveTransition();
				break;
			}
		}

	}

	public class SlideLeftRightNavigationDelegate:UINavigationControllerDelegate{

		bool reversed=false;
		public static UIPercentDrivenInteractiveTransition transition;

		public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation (UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
		{
			reversed = operation == UINavigationControllerOperation.Pop;
			//We need to return "something" that is not null so that the "GetInteractionControllerForAnimationController" is called
			return new CustomTransition ();
		}

		public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForAnimationController (UINavigationController navigationController, IUIViewControllerAnimatedTransitioning animationController)
		{
			transition=new SlideLeftRight (reversed);
			return transition;
		}
	}

	/// <summary>
	/// Created just to ensure that GetInteractionControllerForAnimationController is called
	/// </summary>
	public class CustomTransition : UIViewControllerAnimatedTransitioning
	{
		public override void AnimateTransition (IUIViewControllerContextTransitioning transitionContext)
		{

		}
		public override double TransitionDuration (IUIViewControllerContextTransitioning transitionContext)
		{
			return 0.5;
		}
	}

	public class SlideLeftRight : UIPercentDrivenInteractiveTransition
	{
		bool reverse = false; 
		float length = 0.5f; 
		IUIViewControllerContextTransitioning context;
		bool animationStarted=false;
		nfloat lastPercent=1f;

		public SlideLeftRight ()
		{
		}
		public SlideLeftRight (float length)
		{
			this.length = length; 
		}
		public SlideLeftRight(bool reverse) {
			this.reverse = reverse; 
		}
		public SlideLeftRight(float length, bool reverse) {
			this.reverse = reverse; 
			this.length = length; 
		}
		public override nfloat Duration {
			get {
				return length;
			}
		}

		//Called when a Push/Pop/Swipe back happens
		public override void StartInteractiveTransition (IUIViewControllerContextTransitioning transitionContext){
			context = transitionContext;
			lastPercent = 1f;
			animationStarted = true;
			var inView = transitionContext.ContainerView;
			var fromVC = transitionContext.GetViewControllerForKey (UITransitionContext.FromViewControllerKey);
			var fromView = fromVC.View;
			var toVC = transitionContext.GetViewControllerForKey (UITransitionContext.ToViewControllerKey);
			var toView = toVC.View;

			var frame = toView.Frame;

			if (reverse) {
				inView.InsertSubviewBelow (toView, fromVC.View); 
				toView.Frame = new CGRect (-inView.Frame.Width, 0, frame.Width, frame.Height);
			} else {
				inView.AddSubview (toView);
				toView.Frame = new CGRect (inView.Frame.Width, 0, frame.Width, frame.Height);
			}

			UIView.Animate (Duration,0,UIViewAnimationOptions.AllowUserInteraction, () => {
				if (reverse) {
					fromView.Frame = new CGRect (inView.Frame.Width, 0, frame.Width, frame.Height);
					toView.Frame = new CGRect (0, 0, frame.Width, frame.Height);
				} else {
					fromView.Frame = new CGRect (-inView.Frame.Width, 0, frame.Width, frame.Height);
					toView.Frame = new CGRect (0, 0, frame.Width, frame.Height);
				}
			}, () => {
				if (animationStarted){
					context.FinishInteractiveTransition();
					context.CompleteTransition (true);
					context=null;
				}
				animationStarted=false;
			});
		}

		//Called on HandleOnstagePan while user drags to the older view
		public override void UpdateInteractiveTransition (nfloat percentComplete)
		{
			if (context != null) {
				lastPercent = percentComplete;
				var inView = context.ContainerView;
				var fromVC = context.GetViewControllerForKey (UITransitionContext.FromViewControllerKey);
				var fromView = fromVC.View;
				var toVC = context.GetViewControllerForKey (UITransitionContext.ToViewControllerKey);
				var toView = toVC.View;
				var frame = toView.Frame;

				if (animationStarted) {
					animationStarted = false;

					fromView.Layer.RemoveAllAnimations ();
					toView.Layer.RemoveAllAnimations ();
				}
				fromView.Frame = new CGRect (percentComplete * inView.Frame.Width, 0, frame.Width, frame.Height);
				toView.Frame = new CGRect (percentComplete * inView.Frame.Width - inView.Frame.Width, 0, frame.Width, frame.Height);
			}
		}

		//Called on HandleOnstagePan once a swipe back happened
		public override void FinishInteractiveTransition ()
		{
			if (context != null) {
				var inView = context.ContainerView;
				var fromVC = context.GetViewControllerForKey (UITransitionContext.FromViewControllerKey);
				var fromView = fromVC.View;
				var toVC = context.GetViewControllerForKey (UITransitionContext.ToViewControllerKey);
				var toView = toVC.View;
				var frame = toView.Frame;

				UIView.Animate (Duration,0,UIViewAnimationOptions.AllowUserInteraction, () => {
					//Are we going to pop the view or has it been canceled due to "unfinished" drag?
					if (lastPercent>0.5) {
						fromView.Frame = new CGRect (inView.Frame.Width, 0, frame.Width, frame.Height);
						toView.Frame = new CGRect (0, 0, frame.Width, frame.Height);
					} else {
						fromView.Frame = new CGRect (0, 0, frame.Width, frame.Height);
						toView.Frame = new CGRect (-inView.Frame.Width, 0, frame.Width, frame.Height);
					}
				}, () => {
					context.FinishInteractiveTransition();
					context.CompleteTransition (lastPercent>0.5);
					animationStarted=false;
				});
			}
		}
	}
}

