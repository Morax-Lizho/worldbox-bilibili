using System;
using UnityEngine;
using UnityEngine.UI;

using ReflectionUtility;
using life.taxi;



namespace Bilibili 
{
    public enum UnitJobType
    {
        None,
        King,   // 国王
        Leader, // 侯爵
        GroupLeader,    // 队长
    }

    public static class UnitHandler
    {

        public static UnitJobType GetJobType(this Unit self)
        {
            ActorStatus actorStatus = Reflection.GetField(self.actor.GetType(),self.actor,"data") as ActorStatus;
            switch(actorStatus.profession)
            {
                case UnitProfession.King:
                // 国王
                return UnitJobType.King;
                case UnitProfession.Leader:
                // 领袖
                return UnitJobType.Leader;
                default:
                break;
            }

            var unitGroup = Reflection.GetField(self.actor.GetType(),self.actor,"unitGroup") as UnitGroup;
            if(unitGroup != null && unitGroup.groupLeader == self.actor)
            {
                return UnitJobType.GroupLeader;
            }
            return UnitJobType.None;
        }

        public static void GoTo(this Actor self,WorldTile tile, bool pPathOnWater = false, bool pWalkOnBlocks = false)
        {
            self.cancelAllBeh(null);
            self.commandTargetTile = tile;
            self.ai.setTask("actor_move_to_target",true,true);

            return;
            /*
            self.cancelAllBeh(null);
            if(tile.isSameIsland(self.currentTile))
            {
                // 在同一个岛屿
                Debug.Log("同一个岛屿");
                Reflection.SetField<WorldTile>(self, "beh_tile_target", tile);
                self.goTo(tile, false, false);
            }else{
                Debug.Log("不同岛屿");
                // 不同的岛屿，需要做船
                Reflection.SetField<TileZone>(self.city,"settleTarget",tile.zone);
                self.ai.setTask("settler_check_transport",true,true);
                //TaxiManager.newRequest(self,tile);
                //behTaxiFindShipTile.execute(self);


            }
            */
        }


        // 自己一个人前往某地
        public static void GoTo(this Unit self,WorldTile tile)
        {
            self.actor.GoTo(tile,false,false);
        }
        

        // 一队人，前往某地
        public static void MoveToTile(this UnitGroup self,WorldTile tile)
        {
            var units = Reflection.GetField(self.GetType(),self,"units") as ActorContainer;
            if(units.Count == 0)
            {
                return;
            }
            foreach (Actor actor in units.getSimpleList())
            {
                actor.GoTo(tile, false, false);
            }
        }

        // 移动到指定位置，一群人
        public static void MoveTo(this Unit self,WorldTile tile)
        {
            var city = self.actor.city;
            if(city != null && city.army != null)
            {
                if(city.army.groupLeader == self.actor)
                {
                    city.army.MoveToTile(tile);
                    return;
                }
                if(city.leader == self.actor)
                {
                    city.army.MoveToTile(tile);
                    self.GoTo(tile);
                    return;
                }
            }
    
            // 没有职位
            self.GoTo(tile);
        }

        // 攻击国家
        public static void ToAttackKingdom(this Unit self,MKingdom targetKingdom)
        {
            if(self.actor.kingdom == null)
            {
                return;
            }
            var kingdom = MKingdomManager.instance.GetByKey(self.actor.kingdom.id);
            if(kingdom == null)
            {
                return;
            }
            if(kingdom == targetKingdom)
            {
                return;
            }
            if(kingdom.IsEnemy(targetKingdom) == false)
            {
                // 不是敌对的
                MapBox.instance.addNewText("无法对非敌对国家发起攻击！", Toolbox.color_log_warning, null);
                return;
            }

            // 开始进攻
            WorldTile worldTile = self.actor.currentTile;
            WorldTile cityTile =  Reflection.GetField(targetKingdom.kingdom.capital.GetType(),targetKingdom.kingdom.capital,"_cityTile") as WorldTile;
            foreach (City city2 in  targetKingdom.kingdom.cities)
            {
                var _city2Tile = Reflection.GetField(city2.GetType(),city2,"_cityTile") as WorldTile;
                if (_city2Tile == null || Toolbox.DistVec2(worldTile.pos, _city2Tile.pos) < Toolbox.DistVec2(worldTile.pos, cityTile.pos))
                {
                    cityTile = _city2Tile;
                }
            }
            if (cityTile != null)
            {
                if(self.actor.city != null)
                {
                    self.actor.city.target_attack_zone = cityTile.zone;
                }
                self.MoveTo(cityTile);
            }
        }

        // 移动到国家
        public static void MoveToKingdom(this Unit self,MKingdom targetKingdom)
        {
            WorldTile cityTile =  Reflection.GetField(targetKingdom.kingdom.capital.GetType(),targetKingdom.kingdom.capital,"_cityTile") as WorldTile;
            self.MoveTo(cityTile);
        }

        // 回防
        public static void ToBack(this Unit self)
        {
            if(self.actor.city == null)
            {
                return;
            }
            self.MoveTo(Reflection.GetField(self.actor.city.GetType(),self.actor.city,"_cityTile") as WorldTile);
        }

    }
}