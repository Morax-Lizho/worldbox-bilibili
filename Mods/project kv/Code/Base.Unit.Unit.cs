using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;
using ReflectionUtility;

namespace Bilibili
{

    public class Unit
    {
        // 圆形通道
        static public Texture2D RoundChannel;
        // Actor id
        public string Id;
        // Mod内记录的id
        public string unitId;
        public string unitDataInfo;
        // 所属 Playerid
        public long ownerPlayerUid = 0;
        // 改变国家
        public bool changeKingdom = false;
        // 圆形头像
        public Sprite head;
        // object
        public Actor actor;
        public BaseStats actorCurStats;
        public ActorStatus actorData;

        public UIUnit uIUnit = null;
        public UIBloodBar uIBloodBar = null;

        public void Apply()
        {
            var temp_head = head;
            head = null;
            int width = 64;
            int height = 64;
            Texture2D temp = new Texture2D(width,height);
            Task.Run(
                async ()=>{
                    
                    for(int x =0 ;x<width;++x)
                    {
                        for(int y = 0;y<height;++y)
                        {
                            var color = temp_head.texture.GetPixel(x,y);
                            color.a = RoundChannel.GetPixel(x,y).r;
                            Debug.Log( color.a);
                            temp.SetPixel(x,y,color);
                            //temp.SetPixel(x,y,Color.red);
                        }
                    }
                    Main.actions.Enqueue(
                        ()=>{
                            temp.Apply();
                            head = Sprite.Create(temp, new Rect(0,0,temp.width, temp.height), new Vector2(0.5f, 0.5f));
                        }
                    );
                }
            );
            
            //headSpriteRenderer.sprite = head;
            
            Debug.Log("设置 head");

        }

        public void ReflectionUIUnit()
        {
            if(uIUnit == null)
            {
                return;
            }
            var player = PlayerManager.instance.GetByKey(ownerPlayerUid);
            if(player == null)
            {
                return;
            }
            uIUnit.image.sprite = player.headSprite;
            uIUnit.name.text = $"[{unitId}]{player.name}";
            long killNum = player.playerDataInfo.unitDataInfo.killUnitNum;
            killNum += player.playerDataInfo.unitDataInfo.killWarriorNum;
            killNum += player.playerDataInfo.unitDataInfo.killBabyNum;
            uIUnit.kGlK.text = $"{killNum}/{player.playerDataInfo.unitDataInfo.killLeaderNum}/{player.playerDataInfo.unitDataInfo.killKingNum}";
            
            // 本局
            killNum = player.currentUnitDataInfo.killUnitNum;
            killNum += player.currentUnitDataInfo.killWarriorNum;
            killNum += player.currentUnitDataInfo.killBabyNum;
            killNum += player.currentUnitDataInfo.killLeaderNum;
            killNum += player.currentUnitDataInfo.killKingNum;
            uIUnit.thisTimeKD.text = $"{killNum}/{player.currentUnitDataInfo.deathNum}";

            switch(this.GetJobType())
            {
                case UnitJobType.King:
                    // 国王
                    uIUnit.jobImage.sprite = SpriteManager.iconKings;
                    uIUnit.jobImage.color = Color.white;
                break;
                case UnitJobType.Leader:
                    // 领袖
                    uIUnit.jobImage.sprite = SpriteManager.iconLeaders;
                    uIUnit.jobImage.color = Color.white;
                break;
                case UnitJobType.GroupLeader:
                    uIUnit.jobImage.sprite = SpriteManager.map_mark_flag;
                    uIUnit.jobImage.color = Color.white;
                break;
                default:
                    uIUnit.jobImage.color = Color.clear;
                break;
            }
        }

        public void SecondsUpdate()
        {
            ReflectionUIUnit();
        }

        public void LateUpdate()
        {
            if(uIBloodBar != null)
            {
                //Vector2 pos = GameHelper.MapText.TransformPosition(actor.currentTile.posV3);
                Vector2 pos = GameHelper.MapText.TransformPosition(actor.currentPosition);
                uIBloodBar.rootRect.anchoredPosition = pos;
                uIBloodBar.hpMax = actorCurStats.health;
                uIBloodBar.hp = actorData.health;
                uIBloodBar.RefreshDisplay();
            }
        }
    }

}