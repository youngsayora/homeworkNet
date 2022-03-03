using System.Collections.Generic;

namespace EFCoreExample.DataAccess.Entity
{
	public class Room
	{
		public int Id { get; set; }
		public string RoomName { get; set; }

		public ICollection<Booking> Bookings { get; set; }
	}
}