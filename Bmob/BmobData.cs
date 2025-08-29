using System;
using System.Linq;
using cn.bmob.io;
using Gameplay.Script.Data;
using Script.Core.Tools;
using UnityEngine;

namespace Gameplay.Script.Bmob
{
    public class BmobData
    {
        
    }
    
    public class MiniWorldUser : BmobTable
    {
        public BmobInt uuid;
        public String relevanceId;
        public String name;
        public String headUrl;
        public BmobInt gender;
        public String sessionToken;
        public String regionId;
        public BmobInt second;
        public BmobInt secondmode1;
        public BmobInt secondmode2;
        public BmobInt secondmode3;
        public BmobInt point;
        public String bagPlants;
        public String bagPlantsMode2;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.uuid = input.getInt("uuid");
            this.relevanceId = input.getString("relevanceId");
            this.name = input.getString("name");
            this.headUrl = input.getString("headUrl");
            this.gender = input.getInt("gender");
            this.sessionToken = input.getString("sessionToken");
            this.regionId = input.getString("regionId");
            this.second = input.getInt("second");
            this.secondmode1 = input.getInt("secondmode1");
            this.secondmode2 = input.getInt("secondmode2");
            this.secondmode3 = input.getInt("secondmode3");
            this.point = input.getInt("point");
            this.bagPlants = input.getString("bagPlants");
            this.bagPlantsMode2 = input.getString("bagPlantsMode2");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
        
            if (all)
            {
                output.Put("sessionToken", this.sessionToken);
            }
            output.Put("uuid", this.uuid);
            output.Put("relevanceId", this.relevanceId);
            output.Put("name", this.name);
            output.Put("headUrl", this.headUrl);
            output.Put("gender", this.gender);
            output.Put("regionId", this.regionId);
            output.Put("second", this.second);
            output.Put("secondmode1", this.secondmode1);
            output.Put("secondmode2", this.secondmode2);
            output.Put("secondmode3", this.secondmode3);
            output.Put("point", this.point);
            output.Put("bagPlants", this.bagPlants);
            output.Put("bagPlantsMode2", this.bagPlantsMode2);
        }
    }
    
    public class MiniWorldShopData : BmobTable
    {
        public BmobInt index;
        public String itemId;
        public String otherId;
        public String itemName;
        public String itemType;
        public BmobInt itemCoin;
        public BmobInt itemJewel;
        public BmobInt itemCount;
        public BmobInt visible;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.index = input.getInt("index");
            this.itemId = input.getString("itemId");
            this.otherId = input.getString("otherId");
            this.itemName = input.getString("itemName");
            this.itemType = input.getString("itemType");
            this.itemCoin = input.getInt("itemCoin");
            this.itemJewel = input.getInt("itemJewel");
            this.itemCount = input.getInt("itemCount");
            this.visible = input.getInt("visible");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("index", this.index);
            output.Put("itemId", this.itemId);
            output.Put("otherId", this.otherId);
            output.Put("itemName", this.itemName);
            output.Put("itemType", this.itemType);
            output.Put("itemCoin", this.itemCoin);
            output.Put("itemJewel", this.itemJewel);
            output.Put("itemCount", this.itemCount);
            output.Put("visible", this.visible);
        }
    }
    
    public class MiniWorldUserGameData : BmobTable
    {
        public BmobInt version;
        public String relevanceId;
        public String userPlantJson;
        public BmobInt coin;
        public BmobInt jewel;
        public BmobInt exp;
        public String userAchievements;
        public String userSubscribe;
        public String signIn;
        public String userBagPlants;
        public String userBagPlantsM2;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.version = input.getInt("version");
            this.relevanceId = input.getString("relevanceId");
            this.userPlantJson = input.getString("userPlantJson");
            this.coin = input.getInt("coin");
            this.jewel = input.getInt("jewel");
            this.exp = input.getInt("exp");
            this.userSubscribe = input.getString("userSubscribe");
            this.signIn = input.getString("signIn");
            this.userAchievements = input.getString("userAchievements");
            this.userBagPlants = input.getString("userBagPlants");
            this.userBagPlantsM2 = input.getString("userBagPlantsM2");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("relevanceId", this.relevanceId);
            output.Put("version", this.version);
            output.Put("userPlantJson", this.userPlantJson);
            output.Put("coin", this.coin);
            output.Put("jewel", this.jewel);
            output.Put("exp", this.exp);
            output.Put("userSubscribe", this.userSubscribe);
            output.Put("signIn", this.signIn);
            output.Put("userAchievements", this.userAchievements);
            output.Put("userBagPlants", this.userBagPlants);
            output.Put("userBagPlantsM2", this.userBagPlantsM2);
        }

        public UserDataJson ToJson()
        {
            UserPlantData[] plantJson = null;
            UserSubscribeDataJson[] subscribeJson = null;
            try
            {
                plantJson = AndroidJsonTool.ReadJsonString<UserPlantData[]>(userPlantJson);
                subscribeJson = AndroidJsonTool.ReadJsonString<UserSubscribeDataJson[]>(userSubscribe);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return new UserDataJson()
            {
                objectId = objectId,
                version = version?.Get() ?? 0,
                relevanceId = relevanceId,
                userPlants = plantJson,
                coin = coin?.Get() ?? 0,
                jewel = jewel?.Get() ?? 0,
                exp = exp?.Get() ?? 0,
                userSubscribeJson = subscribeJson,
                signIn = string.IsNullOrEmpty(signIn) ? Array.Empty<string>() : signIn.Split(','),
                userAchievements = string.IsNullOrEmpty(userAchievements) 
                    ? Array.Empty<string>() : signIn.Split(','),
                userBagPlants = string.IsNullOrEmpty(userBagPlants) 
                    ? Array.Empty<int>() 
                    : userBagPlants.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToArray(),
                userBagPlantsM2 = string.IsNullOrEmpty(userBagPlantsM2)
                    ? Array.Empty<int>()
                    : userBagPlantsM2.Split(',')
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToArray()
            };
        }
    }
    
    public class MiniWorldUserRankingData : BmobTable
    {
        public String relevanceId;
        public BmobInt fightingCapacity;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.relevanceId = input.getString("relevanceId");
            this.fightingCapacity = input.getInt("fightingCapacity");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("relevanceId", this.relevanceId);
            output.Put("fightingCapacity", this.fightingCapacity);
        }
    }
    
    public class MiniWorldUNotice : BmobTable
    {
        public String version;
        public BmobInt index;
        public String title;
        public String content;
        public BmobInt valid;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.version = input.getString("version");
            this.index = input.getInt("index");
            this.title = input.getString("title");
            this.content = input.getString("content");
            this.valid = input.getInt("valid");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("version", this.version);
            output.Put("index", this.index);
            output.Put("title", this.title);
            output.Put("content", this.content);
            output.Put("valid", this.valid);
        }
    }
    
    public class MiniWorldUserStoreData : BmobTable
    {
        public BmobInt version;
        public String relevanceId;
        public String purchaseIds;
        public String goodsJson;
        public String unlockJson;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.version = input.getInt("version");
            this.relevanceId = input.getString("relevanceId");
            this.goodsJson = input.getString("goodsJson");
            this.unlockJson = input.getString("unlockJson");
            this.purchaseIds = input.getString("purchaseIds");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("relevanceId", this.relevanceId);
            output.Put("version", this.version);
            output.Put("goodsJson", this.goodsJson);
            output.Put("unlockJson", this.unlockJson);
            output.Put("purchaseIds", this.purchaseIds);
        }

        public UserStoreJson ToJson()
        {
            UserGoodsJson json = null;
            if (!string.IsNullOrEmpty(goodsJson))
            {
                try
                {
                    json = AndroidJsonTool.ReadJsonString<UserGoodsJson>(goodsJson);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            
            UserUnlockItemsJson json1 = null;
            if (!string.IsNullOrEmpty(unlockJson))
            {
                try
                {
                    json1 = AndroidJsonTool.ReadJsonString<UserUnlockItemsJson>(unlockJson);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            return new UserStoreJson()
            {
                objectId = objectId,
                version = version?.Get() ?? 0,
                relevanceId = relevanceId,
                purchaseIds = purchaseIds,
                userGoodsJson = json ?? new UserGoodsJson().Create(),
                userUnlockItemsJson = json1 ?? new UserUnlockItemsJson().Create(),
            };
        }
    }
    
    public class MiniWorldUnlockItemData : BmobTable
    {
        public BmobInt id;
        public String names;
        public String versions;
        public String itemIds;
        public String startTime;
        public String endTime;
        public BmobInt needTime;
        public BmobInt visible;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.id = input.getInt("id");
            this.names = input.getString("names");
            this.versions = input.getString("versions");
            this.itemIds = input.getString("itemIds");
            this.startTime = input.getString("startTime");
            this.endTime = input.getString("endTime");
            this.needTime = input.getInt("needTime");
            this.visible = input.getInt("visible");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("id", this.id);
            output.Put("names", this.names);
            output.Put("versions", this.versions);
            output.Put("itemId", this.itemIds);
            output.Put("startTime", this.startTime);
            output.Put("endTime", this.endTime);
            output.Put("needTime", this.needTime);
            output.Put("visible", this.visible);
        }
    }
    
    public class MiniWorldRanking : BmobTable
    {
        public BmobInt index;
        public String relevanceId;
        public BmobInt defaultSecond;
        public BmobInt fastModeSecond;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.index = input.getInt("index");
            this.relevanceId = input.getString("relevanceId");
            this.defaultSecond = input.getInt("defaultSecond");
            this.fastModeSecond = input.getInt("fastModeSecond");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("index", this.index);
            output.Put("relevanceId", this.relevanceId);
            output.Put("defaultSecond", this.defaultSecond);
            output.Put("fastModeSecond", this.fastModeSecond);
        }
    }

    public class MiniWorldActivity : BmobTable
    {
        public BmobInt index;
        public String type;
        public String title;
        public String description;
        public String formula;
        public String award;
        public String version;
        public BmobInt visible;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.index = input.getInt("index");
            this.type = input.getString("type");
            this.title = input.getString("title");
            this.description = input.getString("description");
            this.formula = input.getString("formula");
            this.award = input.getString("award");
            this.version = input.getString("version");
            this.visible = input.getInt("visible");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("index", this.index);
            output.Put("type", this.type);
            output.Put("title", this.title);
            output.Put("description", this.description);
            output.Put("formula", this.formula);
            output.Put("award", this.award);
            output.Put("version", this.version);
            output.Put("visible", this.visible);
        }
    }
    
    public class MiniWorldPlant : BmobTable
    {
        public BmobInt index;
        public String plantId;
        public String plantName;
        public String plantType;
        public String plantAttribute;
        public String plantQuality;
        public String plantIcon;
        public String plantTexture;
        public String plantVersion;
        public String plantDescription;
        public String plantSkills;
        public String packUrl;
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.index = input.getInt("index");
            this.plantId = input.getString("plantId");
            this.plantName = input.getString("plantName");
            this.plantType = input.getString("plantType");
            this.plantAttribute = input.getString("plantAttribute");
            this.plantQuality = input.getString("plantQuality");
            
            this.plantIcon = input.getString("plantIcon");
            this.plantTexture = input.getString("plantTexture");
            this.plantVersion = input.getString("plantVersion");
            this.plantDescription = input.getString("plantDescription");
            this.plantSkills = input.getString("plantSkills");
            this.packUrl = input.getString("packUrl");
        }

        public override void write(BmobOutput output, Boolean all)
        {
            base.write(output, all);
            output.Put("index", this.index);
            output.Put("plantId", this.plantId);
            output.Put("plantName", this.plantName);
            output.Put("plantType", this.plantType);
            output.Put("plantAttribute", this.plantAttribute);
            output.Put("plantQuality", this.plantQuality);
            output.Put("plantIcon", this.plantIcon);
            output.Put("plantTexture", this.plantTexture);
            output.Put("plantVersion", this.plantVersion);
            output.Put("plantDescription", this.plantDescription);
            output.Put("plantSkills", this.plantSkills);
            output.Put("packUrl", this.packUrl);
        }
    }
}