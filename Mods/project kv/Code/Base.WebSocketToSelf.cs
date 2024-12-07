using Newtonsoft.Json.Linq;
using Proyecto26;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using UnityEngine;

namespace Bilibili  
{
    public class WebSocketToSelf
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        enum MsgType
        {
            Barrage = 1,    // 弹幕
            Gift = 2,   // 礼物
            FocusOn = 3,    // 关注
            Enter = 4,  // 进入直播间
        }

        //public class MsgBase
        //{
        //    [JsonProperty("uid")]
        //    public long uid = 0;
        //    [JsonProperty("uname")]
        //    public string uname = "";
        //    [JsonProperty("type")]
        //    public int type = 0;
        //    [JsonProperty("headurl")]
        //    public string headurl = "";
        //    [JsonProperty("timestamp")]
        //    public long timestamp = 0;
        //    // 粉丝牌
        //    [JsonProperty("medal_name")]
        //    public string medal_name = "";
        //    [JsonProperty("medal_room_id")]
        //    public string medal_room_id = "";
        //    [JsonProperty("medal_level")]
        //    public string medal_level = "";

        //}

        //public class BarrageMsg
        //{
        //    [JsonProperty("msg")]
        //    public string msg = "";
        //}

        //public class GiftMsg
        //{
        //    [JsonProperty("gif_id")]
        //    public int gif_id = 0;
        //    [JsonProperty("gif_name")]
        //    public string gif_name = "";
        //    [JsonProperty("gif_num")]
        //    public int gif_num = 0;
        //    [JsonProperty("gif_price")]
        //    public int gif_price = 0;
        //}


        private WSocketClientHelp client;
        public WebSocketToSelf()
        {
        }

        ~WebSocketToSelf()
        {
        }

        public void Update()
        {
            client.Update();
        }

        public void Connect(string url)
        {

            client = new WSocketClientHelp(url);
            client.OnMessage += DistributeMessage;
            client.Open();
        }

        public string GetResponse(string Url)
        {
            string ResponseData = string.Empty;
            try
            {

                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(Url);
                httpWebRequest.Timeout = 200000;
                httpWebRequest.Method = "GET";

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                ResponseData = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

            }
            catch (Exception)
            {
                ResponseData = null;
            }
            return ResponseData;
        }

        public string Get_detail(string url)
        {
            string reponse = null;
            try
            {
                reponse = GetResponse(url);

            }
            catch (Exception)
            {
                reponse = string.Empty;
            }

            return reponse;
        }


        /// <summary>
        /// 从json中获取对应key的value值
        /// </summary>
        /// <param name="json字符串"></param>
        /// <param name="需要取value对应的key"></param>
        /// <returns></returns>
        public string GetJsonValue(string strJson, string key)
        {
            //测试：
            //strJson = @"{'1':{'id':{'ip':'192.168.0.1','p':34,'pass':'ff','port':80,'user':'t'}},'code':0}";
            //key = "user"
            string strResult = "";
            JObject jsonObj = JObject.Parse(strJson);
            strResult = GetNestJsonValue(jsonObj.Children(), key);
            return strResult;
        }

        /// <summary>
        /// 迭代获取eky对应的值
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetNestJsonValue(JEnumerable<JToken> jToken, string key)
        {
            IEnumerator enumerator = jToken.GetEnumerator();
            while (enumerator.MoveNext())
            {
                JToken jc = (JToken)enumerator.Current;
                if (jc is JObject || ((JProperty)jc).Value is JObject)
                {
                    return GetNestJsonValue(jc.Children(), key);
                }
                else
                {
                    if (((JProperty)jc).Name == key)
                    {
                        return ((JProperty)jc).Value.ToString();
                    }
                }
            }
            return null;
        }

        public void DistributeMessage(object sender, string data)
        {
            Debug.Log(data);
            //var msgBase = JsonConvert.DeserializeObject(data, typeof(MsgBase)) as MsgBase;
            PlayerInfo info = new PlayerInfo();
            var tu = data.Split(separator: ',');
            info.uid = long.Parse(data.Split(separator: ',')[1]);
            info.name = data.Split(separator: ',')[2];

            
            info.urlHead = GetJsonValue(Get_detail($"https://api.bilibili.com/x/space/acc/info?mid={info.uid}"), "face");
            info.medal_name = data.Split(separator: ',')[4];
            info.medal_room_id = data.Split(separator: ',')[5];
            int.TryParse(data.Split(separator: ',')[6], out info.medal_level);


            switch (data.Split(separator: ',')[0])
            {
                case "1":
                    {
                        //var barrageMsg = JsonConvert.DeserializeObject(data, typeof(BarrageMsg)) as BarrageMsg;
                        Debug.Log($"[{data.Split(separator: ',')[1]}:{data.Split(separator: ',')[2]}]{data.Split(separator: ',')[7]}");
                        MessageDistribute.NormalMsg msg = new MessageDistribute.NormalMsg();
                        msg.msg = data.Split(separator: ',')[7];
                        if (MessageDistribute.instance == null)
                        {
                            Debug.Log("MessageDistribute.instance == null");
                        }
                        MessageDistribute.instance.DistributeNormalMsg(info, msg);
                    }
                    break;
                case "2":
                    {
                        //var giftMsg = JsonConvert.DeserializeObject(data, typeof(GiftMsg)) as GiftMsg;
                        MessageDistribute.GiftMsg msg = new MessageDistribute.GiftMsg();
                        msg.giftId = int.Parse(data.Split(separator: ',')[3]);
                        msg.giftName = data.Split(separator: ',')[4];
                        msg.giftNum = int.Parse(data.Split(separator: ',')[5]);
                        msg.giftPrice = int.Parse(data.Split(separator: ',')[6]);
                        MessageDistribute.instance.DistributeGiftMsg(info, msg);
                    }
                    break;
            }

        }

        //public void Log(string msg)
        //{
        //    Debug.Log(msg);
        //}

        


    }
}
