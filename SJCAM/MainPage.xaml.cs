﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SJCAM.Effects;
using SJCAM.MainGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SJCAM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		Color Dominant;
		public string Title { get; set; }
		public string Description { get; set; }
		public ObservableCollection<Clickable> Menu { get; set; }
		public ObservableCollection<SubMenu> Sub { get; set; }
		private float currentBlur = 0;
		private CompositionEffectBrush _brush;
		private Compositor _compositor;

		public MainPage()
        {
			InitAsync();
			this.InitializeComponent();
			Title = "SJCAM";	
			Description = "Welcome!";
			_compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
			CreateMenu();
		}

		private void CreateMenu()
		{
			Menu = new ObservableCollection<Clickable>();
			Menu.Add(new Clickable("Photo", "\uE114" ));
			Menu.Add(new Clickable("Video", "\uE116" ));
			Menu.Add(new Clickable("Settings", "\uE179" ));
			Menu.Add(new Clickable("Other", "\uE188" ));
		}

		private async void InitAsync()
		{
			Dominant = await Images.GetDominant();
			var view = ApplicationView.GetForCurrentView();
			view.TitleBar.BackgroundColor = Dominant;
			view.TitleBar.ButtonBackgroundColor = Dominant;
			view.ExitFullScreenMode();
			view.ShowStandardSystemOverlays();
			SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
		}

		private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
		{
			e.Handled = true;
			StartBlurAnimation();
			ShowContent("Menu");
			await AdaptiveGridViewControl.Fade(1, 500, 500).StartAsync();
		}

		private void Blur()
		{
			var graphicsEffect = new GaussianBlurEffect()
			{
				Name = "Blur",
				Source = new CompositionEffectSourceParameter("Backdrop"),
				BlurAmount = 50,
				BorderMode = EffectBorderMode.Hard,
			};
			var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "Blur.BlurAmount" });

			_brush = blurEffectFactory.CreateBrush();
			var destinationBrush = _compositor.CreateBackdropBrush();
			_brush.SetSourceParameter("Backdrop", destinationBrush);
			var blurSprite = _compositor.CreateSpriteVisual();
			blurSprite.Size = new Vector2((float)backgroundImage.ActualWidth, (float)backgroundImage.ActualHeight);
			blurSprite.Brush = _brush;
			ElementCompositionPreview.SetElementChildVisual(backgroundImage, blurSprite);
		}

		private void BackgroundImage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SpriteVisual blurVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(backgroundImage);
			if (blurVisual != null)
				blurVisual.Size = e.NewSize.ToVector2();
		}

		private void StartBlurAnimation(float _toBlurAmount = 10)
		{
			ScalarKeyFrameAnimation blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
			blurAnimation.InsertKeyFrame(0.0f, currentBlur);
			blurAnimation.InsertKeyFrame(1.0f, _toBlurAmount);
			currentBlur = _toBlurAmount;
			blurAnimation.Duration = TimeSpan.FromSeconds(2);
			blurAnimation.IterationBehavior = AnimationIterationBehavior.Count;
			blurAnimation.IterationCount = 1;
			_brush.StartAnimation("Blur.BlurAmount", blurAnimation);
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void BackgroundImage_Loaded(object sender, RoutedEventArgs e)
		{
			Blur();
			_brush.Properties.InsertScalar("Blur.BlurAmount", 0);
			StartBlurAnimation();
		}

		private async void AdaptiveGridViewControl_ItemClickAsync(object sender, ItemClickEventArgs e)
		{
			StartBlurAnimation(50f);
			await (sender as AdaptiveGridView).Fade().StartAsync();

			string currentName = (e.ClickedItem as Clickable).Name;
			foreach (var item in AdaptiveGridViewControl.Items)
				if ((item as Clickable).Name == currentName)
					ShowContent(currentName);
		}

		private void ShowContent(string currentName)
		{
			switch (currentName)
			{
				case "Photo":
					LoadPhoto();
					break;
				case "Video":
					LoadVideo();
						break;
				case "Settings":
					LoadSettings();
					break;
				default:
					HideCurrent();
					break;
			}
		}

		private async void HideCurrent()
		{
			await player.Fade().StartAsync();
			player.Visibility = Visibility.Collapsed;
			await SubMenuStack.Fade().StartAsync();
			SubMenuStack.Visibility = Visibility.Collapsed;
		}

		private async void LoadPhoto()
		{
			Sub = new ObservableCollection<SubMenu>();
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			SubMenuStack.Visibility = Visibility.Visible;
			await SubMenuStack.Fade(1,1000,0).StartAsync();
		}

		private async void LoadVideo()
		{
			Sub = new ObservableCollection<SubMenu>();
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			SubMenuStack.Visibility = Visibility.Visible;
			await SubMenuStack.Fade(1,1000,0).StartAsync();
		}

		private async void LoadSettings()
		{
			Sub = new ObservableCollection<SubMenu>();
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			Sub.Add(new SubMenu("test"));
			SubMenuStack.Visibility = Visibility.Visible;
			await SubMenuStack.Fade(1,1000,0).StartAsync();
		}

		private void MenuTapped(object sender, RoutedEventArgs e)
		{
			
		}
	}
}