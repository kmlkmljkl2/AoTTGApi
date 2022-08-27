using AoTTGApi.CarrierClasses;
using AottgBotLib;
using ExitGames.Client.Photon;
using Microsoft.AspNetCore.Mvc;

namespace AoTTGApi.Controllers
{
    [ApiController]
    [Route("api/aottg")]
    public class AoTTGController : ControllerBase
    {
        public Dictionary<string, AottgBotLib.PhotonRegion> Region { get; set; } = new Dictionary<string, AottgBotLib.PhotonRegion>()
        {
            { "europe", AottgBotLib.PhotonRegion.Europe },
            { "eu", AottgBotLib.PhotonRegion.Europe },
            { "sa", AottgBotLib.PhotonRegion.SA },
            { "us", AottgBotLib.PhotonRegion.USA },
            { "usa", AottgBotLib.PhotonRegion.USA },
            { "asia", AottgBotLib.PhotonRegion.Asia },
        };

        [HttpGet("GetRoomList/{region}")]
        public ActionResult<IEnumerable<RoomInfo>> Get(string region)
        {
            try
            {
                region = region.ToLower();
                if (!Region.ContainsKey(region))
                {
                    return BadRequest("Invalid Region");
                    //return StatusCode(400, "Invalid Region");
                }

                IEnumerable<RoomInfo> result = GetRoomList(Region[region]).GetAwaiter().GetResult();
                var response = new ResponseJson()
                {
                    Rooms = result,
                    RoomCount = result.Count()
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("Region")]
        public ActionResult<IEnumerable<string>> FetchRegions()
        {
            return Ok(Region.Keys.ToList());
        }




        private static async Task<IEnumerable<RoomInfo>> GetRoomList(PhotonRegion region)
        {
            var bot = new BotClient("baka")
            {
                IsUsingPhotonServer = true,
                SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV16,
                AppId = "",
                ConnectField = "01042015_1.28",
                TransportProtocol = ExitGames.Client.Photon.ConnectionProtocol.Udp,
                Region = region
            };
            await bot.ConnectToMasterAsync();
            List<RoomInfo> roomList = new List<RoomInfo>();
            foreach (var i in bot.RoomList)
                roomList.Add(new CarrierClasses.RoomInfo(i.Name, i.MaxPlayers, i.PlayerCount));
            bot.Disconnect();

            return roomList;
        }
    }
}