using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TouchScreenComicViewer {
	public partial class ComicViewingMenu : UserControl {

		public event RoutedEventHandler ComicClosedButtonClicked;
		private double fullHeightMenu = 100;
		public ComicViewingMenu() {
			InitializeComponent();
		}

		public void CloseMenuWithAnimation()
		{
			ExpandY.From = fullHeightMenu;
			ExpandY.To = 0;
			ExpandMenuStoryBoard.Completed += CollapseExpandedMenuAnimationComplete;
			ExpandMenuStoryBoard.Begin();
		}

		public bool IsMenuExpanded() 
		{
			return (this.ActualHeight >= fullHeightMenu);
		}

		private void ComicViewingMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (this.expandedMenu.Visibility == System.Windows.Visibility.Collapsed) {
				this.expandedMenu.Visibility = System.Windows.Visibility.Visible;
				this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
				ExpandY.To = fullHeightMenu;
				ExpandY.From = 0;
				ExpandMenuStoryBoard.Begin();
			} else {
				CloseMenuWithAnimation();
			}
			e.Handled = true;
		}

		private void CloseComicBtn_click (object sender, RoutedEventArgs e) {
			var handler = ComicClosedButtonClicked;
			if (handler != null) {
				handler(sender, e);
			}
			CollapseExpandedMenu();
		}

		private void CollapseExpandedMenu() {
			this.expandedMenu.Visibility = System.Windows.Visibility.Collapsed;
			this.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
		}

		private void CollapseExpandedMenuAnimationComplete(Object sender, EventArgs e)
		{
			CollapseExpandedMenu();
			ExpandMenuStoryBoard.Completed -= CollapseExpandedMenuAnimationComplete;
		}

		private void ComicViewingMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
		}
	}
}
