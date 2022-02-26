using EFCoreExample.DataAccess;
using EFCoreExample.DataAccess.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCoreExample.Controllers
{
	[Route("room")]
	public class RoomController : Controller
	{
		private readonly BookingContext _bookingContext;

		public RoomController(BookingContext bookingContext)
		{
			_bookingContext = bookingContext;
		}
		[HttpGet]
		public async Task<ActionResult<string[]>> GetAllRoomNames()
		{
			return await _bookingContext.Rooms
				.Select(u => u.RoomName)
				.ToArrayAsync();
		}

		[HttpPost]
		public async Task<ActionResult<Room>> CreateRoom([FromBody] Room room)
		{
			Room roomInDb = await _bookingContext
				.Rooms
				.FirstOrDefaultAsync(u => u.RoomName == room.RoomName);

			if (roomInDb != null)
				return Conflict("Room with this name already exists");

			_bookingContext.Rooms.Add(room);
			await _bookingContext.SaveChangesAsync();

			return Ok(room);
		}

		[Route("getFreeRooms")]
		[HttpGet]
		public async Task<ActionResult<string[]>> GetAllRooms(DateTime getFromUtc, DateTime getToUtc)
		{
			List<int> busyRoomIds = await _bookingContext.Bookings
				.Where(p => p.FromUtc <= getFromUtc
				&& p.ToUtc >= getFromUtc
				|| p.FromUtc <= getToUtc
				&& p.ToUtc >= getToUtc
				|| p.FromUtc == getFromUtc
				&& p.ToUtc == getToUtc
				)
				.Select(u => u.RoomId)
				.ToListAsync();
			return await _bookingContext.Rooms
				.Where(r => !busyRoomIds.Contains(r.Id))
				.Select(u => u.RoomName)
				.ToArrayAsync();

		}

	}
}
