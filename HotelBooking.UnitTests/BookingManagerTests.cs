using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
//Jeg bruger små dataklasser
//Jeg laver specifikke testmetoder for expected outcome
//
//
//
namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }
        
        [Theory]
        [MemberData(nameof(BookingManagerData.InvalidDates), 
            MemberType = typeof(BookingManagerData))]
        public async Task FindAvailableRoom_InvalidDates_ThrowsArgumentException(DateTime start, DateTime end)
        {
            Task act() => bookingManager.FindAvailableRoom(start, end);
            await Assert.ThrowsAsync<ArgumentException>(act);
        }
        
        [Theory]                                                                                              
        [MemberData(nameof(BookingManagerData.BookingAvailable),                                         
            MemberType = typeof(BookingManagerData))]                                                
        public async Task FindAvailableRoom_Bookings_DontReturnMinusOne(DateTime start, DateTime end, int notExpected)
        {                                                                                                     
            int roomId = await bookingManager.FindAvailableRoom(start, end);                                       
            Assert.NotEqual(notExpected, roomId);                                                 
        }
        [Theory]
        [MemberData(nameof(BookingManagerData.BookingNotAvailable),                                                             
            MemberType = typeof(BookingManagerData))]                                                                        
        public async Task FindAvailableRoom_NoBookings_ReturnMinusOne(DateTime start, DateTime end, int expected)
        {                                                                                                                    
            int roomId = await bookingManager.FindAvailableRoom(start, end);                                                 
            Assert.Equal(expected, roomId);                                                                            
        }                                                                                                                    

        [Theory]
        [MemberData(nameof(BookingManagerRoomAvailableData.AvailableDates),
            MemberType = typeof(BookingManagerRoomAvailableData))]
        public async Task FindAvailableRoom_RoomAvailable(DateTime start, DateTime end)
        {
            int roomId = await bookingManager.FindAvailableRoom(start, end);
            Assert.NotEqual(-1, roomId);
        }

        [Theory]
        [MemberData(nameof(BookingManagerCreateBookingData.BookingCases),
            MemberType = typeof(BookingManagerCreateBookingData))]
        public async Task CreateBooking_AvailableRoom_ReturnsTrue(DateTime start, DateTime end)
        {
            var booking = new Booking { StartDate = start, EndDate = end };
            bool result = await bookingManager.CreateBooking(booking);
            Assert.True(result);
        }

        
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
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), -1 },
                    new object[] { DateTime.Today.AddDays(2), DateTime.Today.AddDays(3), -1 },
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
                    new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(1) },
                    new object[] { DateTime.Today.AddDays(2), DateTime.Today.AddDays(3) }
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
    
    
    
    
    
    
    
    
    
    
