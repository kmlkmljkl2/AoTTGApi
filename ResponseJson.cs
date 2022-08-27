using AoTTGApi.CarrierClasses;

namespace AoTTGApi
{
    public class ResponseJson
    {
        public IEnumerable<RoomInfo> Rooms { get; set; }
        public int RoomCount { get; set; }
    }
}
