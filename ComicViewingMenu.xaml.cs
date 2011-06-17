﻿using System;
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
		public ComicViewingMenu() {
			InitializeComponent();
			
		}

		private void ComicViewingMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			if (this.expandedMenu.Visibility == System.Windows.Visibility.Collapsed) {
				this.expandedMenu.Visibility = System.Windows.Visibility.Visible;
				this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
			} else {
				this.expandedMenu.Visibility = System.Windows.Visibility.Collapsed;
				this.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
			}
		}
	}
}
