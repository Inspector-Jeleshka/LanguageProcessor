using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GUI;

public static class CustomCommands
{
	public static readonly RoutedCommand ShowAbout = new("ShowAbout", typeof(CustomCommands));
}
