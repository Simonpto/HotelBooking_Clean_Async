using System;
using System.Collections.Generic;
using HotelBooking.Core;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private Mock<IRepository<Booking>> fakeBookingRepository;
        private Mock<IRepository<Room>> fakeRoomRepository;
        private IBookingManager bookingManager;

        private List<Room> rooms;
        private List<Booking> bookings;

        public BookingManagerTests()
        {
            // Arrange test data
            rooms = new List<Room>
            {
                new Room { Id = 1, Description = "A" },
                new Room { Id = 2, Description = "B" },
            };

            bookings = new List<Booking>
            {
                new Booking
                {
                    RoomId = 1,
                    StartDate = DateTime.Today.AddDays(10),
                    EndDate = DateTime.Today.AddDays(20),
                    IsActive = true
                },
                new Booking{
                    RoomId=2, 
                    StartDate = DateTime.Today.AddDays(8),
                    EndDate = DateTime.Today.AddDays(15),
                    IsActive = true 
                }
            };

            // Create fake repositories
            fakeRoomRepository = new Mock<IRepository<Room>>();
            fakeBookingRepository = new Mock<IRepository<Booking>>();

            // Setup GetAllAsync for rooms
            fakeRoomRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(rooms);

            // Setup GetAsync with range matcher
            fakeRoomRepository
                .Setup(x => x.GetAsync(It.IsInRange<int>(1, 2, Moq.Range.Inclusive)))
                .ReturnsAsync((int id) =>
                    rooms.FirstOrDefault(r => r.Id == id));

            // Setup GetAllAsync for bookings
            fakeBookingRepository
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(bookings);

            // Setup AddAsync
            fakeBookingRepository
                .Setup(x => x.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            // Create BookingManager
            bookingManager = new BookingManager(
                fakeBookingRepository.Object,
                fakeRoomRepository.Object);
        }


        [Theory]
        [MemberData(nameof(BookingManagerData.InvalidDates),
            MemberType = typeof(BookingManagerData))]
        public async Task FindAvailableRoom_InvalidDates_ThrowsArgumentException(
            DateTime start,
            DateTime end)
        {
            // Act
            Task act() => bookingManager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(act);

            // Verify
            fakeBookingRepository.Verify(
                x => x.GetAllAsync(),
                Times.Never());

            fakeRoomRepository.Verify(
                x => x.GetAllAsync(),
                Times.Never());
        }


        [Theory]
        [MemberData(nameof(BookingManagerData.BookingAvailable),
            MemberType = typeof(BookingManagerData))]
        public async Task FindAvailableRoom_Bookings_DontReturnMinusOne(
            DateTime start,
            DateTime end,
            int notExpected)
        {
            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.NotEqual(notExpected, roomId);

            // Verify
            fakeBookingRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());

            fakeRoomRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());
        }


        [Theory]
        [MemberData(nameof(BookingManagerData.BookingNotAvailable),
            MemberType = typeof(BookingManagerData))]
        public async Task FindAvailableRoom_NoBookings_ReturnMinusOne(
            DateTime start,
            DateTime end,
            int expected)
        {
            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(expected, roomId);

            // Verify
            fakeBookingRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());

            fakeRoomRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());
        }


        [Theory]
        [MemberData(nameof(BookingManagerRoomAvailableData.AvailableDates),
            MemberType = typeof(BookingManagerRoomAvailableData))]
        public async Task FindAvailableRoom_RoomAvailable(
            DateTime start,
            DateTime end)
        {
            // Act
            int roomId = await bookingManager.FindAvailableRoom(start, end);

            // Assert
            Assert.NotEqual(-1, roomId);

            // Verify
            fakeBookingRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());

            fakeRoomRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());
        }


        [Theory]
        [MemberData(nameof(BookingManagerCreateBookingData.BookingCases),
            MemberType = typeof(BookingManagerCreateBookingData))]
        public async Task CreateBooking_AvailableRoom_ReturnsTrue(
            DateTime start,
            DateTime end)
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = start,
                EndDate = end
            };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);

            // Verify
            fakeBookingRepository.Verify(
                x => x.AddAsync(It.IsAny<Booking>()),
                Times.Once());

            // Verify
            fakeBookingRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());

            fakeRoomRepository.Verify(
                x => x.GetAllAsync(),
                Times.Once());
        }



        // TEST DATA CLASSEr

        public class BookingManagerData
        {
            public static IEnumerable<object[]> InvalidDates =>
                new List<object[]>
                {
                    new object[] { DateTime.Today, DateTime.Today },
                    new object[] { DateTime.Today.AddDays(-1), DateTime.Today }
                };

            public static IEnumerable<object[]> BookingAvailable =>
                new List<object[]>
                {
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), -1 },
                    new object[] { DateTime.Today.AddDays(3), DateTime.Today.AddDays(4), -1 },
                };

            public static IEnumerable<object[]> BookingNotAvailable =>
                new List<object[]>
                {
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(100), -1 }
                };
        }


        public class BookingManagerRoomAvailableData
        {
            public static IEnumerable<object[]> AvailableDates =>
                new List<object[]>
                {
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(2) },
                    new object[] { DateTime.Today.AddDays(3), DateTime.Today.AddDays(4) }
                };
        }


        public class BookingManagerCreateBookingData
        {
            public static IEnumerable<object[]> BookingCases =>
                new List<object[]>
                {
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(2) },
                    new object[] { DateTime.Today.AddDays(3), DateTime.Today.AddDays(4) }
                };
        }
    }
}