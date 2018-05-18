using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	public static class NetworkHelper
	{
		// https://stackoverflow.com/questions/520347/how-do-i-check-for-a-network-connection
		public static bool IsNetworkAvailable ( long minimumSpeed = 0 )
		{
			if ( !NetworkInterface.GetIsNetworkAvailable () )
				return false;

			foreach ( NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces () )
			{
				// discard because of standard reasons
				if ( ( ni.OperationalStatus != OperationalStatus.Up ) ||
					( ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ) ||
					( ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel ) )
					continue;

				// this allow to filter modems, serial, etc.
				// I use 10000000 as a minimum speed for most cases
				if ( ni.Speed < minimumSpeed )
					continue;

				// discard virtual cards (virtual box, virtual pc, etc.)
				if ( ContainsString ( ni, "virtual" ) )
					if ( !( ContainsString ( ni, "Intel" )
						|| ContainsString ( ni, "Realtek" )
						|| ContainsString ( ni, "Killer" )
						|| ContainsString ( ni, "Broadcom" ) ) )
						continue;

				// discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
				if ( ni.Description.Equals ( "Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase ) )
					continue;

				return true;
			}
			return false;
		}

		private static bool ContainsString ( NetworkInterface ni, string str, StringComparison comparison = StringComparison.OrdinalIgnoreCase )
		{
			return ( ni.Description.IndexOf ( str, comparison ) >= 0 ) || ( ni.Name.IndexOf ( str, comparison ) >= 0 );
		}
	}
}
