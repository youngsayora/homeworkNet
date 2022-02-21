﻿using EFCoreExample.DataAccess;
using EFCoreExample.DataAccess.Entity;
using EFCoreExample.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreExample.Controllers
{
	[Route("booking")]
	public class BookingController : Controller
	{
		private readonly BookingContext _bookingContext;

		public BookingController(BookingContext bookingContext)
		{
			_bookingContext = bookingContext;
		}

		public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
		{
			var bookings = await _bookingContext.Bookings
				.Include(b => b.User)
				.ToArrayAsync(cancellationToken);
			return Ok(bookings.Select(b => BookingDto.FromBooking(b)));
		}

		[HttpPost]
		public async Task<ActionResult<BookingDto>> CreateBooking(BookingDto bookingDto)
		{
			if (string.IsNullOrEmpty(bookingDto.Username))
				return BadRequest("Username cannot be empty");
			if (bookingDto.FromUtc == default)
				return BadRequest("FromUtc cannot be empty");
			if (bookingDto.ToUtc == default)
				return BadRequest("ToUtc cannot be empty");

			User user = await _bookingContext
				.Users
				.FirstOrDefaultAsync(u => u.UserName == bookingDto.Username);
			if (user is null)
				return BadRequest($"User with name '{bookingDto.Username}' cannot be found");

			Booking newBooking = bookingDto
				.ToBooking(userId: user.Id);
			var alreadyCreatedBooking = await _bookingContext
				.Bookings
				.FirstOrDefaultAsync(b =>
					b.FromUtc <= newBooking.FromUtc
					&& b.ToUtc >= newBooking.FromUtc
					|| b.FromUtc <= newBooking.ToUtc
					&& b.ToUtc >= newBooking.ToUtc
					|| b.FromUtc == newBooking.FromUtc
					&& b.ToUtc == newBooking.ToUtc);
			if (alreadyCreatedBooking is not null)
				return Conflict("Booking for this time has already been created");
			if (newBooking.FromUtc < DateTime.UtcNow)
				return BadRequest("Cannot have from date earlier than now");
			if (newBooking.ToUtc - newBooking.FromUtc <= TimeSpan.FromMinutes(30))
				return BadRequest("Booking period should be at lease 30 minutes long");

			_bookingContext.Add(newBooking);
			await _bookingContext.SaveChangesAsync();
			return Ok(BookingDto.FromBooking(newBooking));
		}
	}
}
