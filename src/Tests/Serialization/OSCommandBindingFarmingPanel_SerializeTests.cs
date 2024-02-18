﻿using DCSFPTests.Serialization.Common;
using Newtonsoft.Json;
using NonVisuals.BindingClasses.OSCommand;
using Xunit;

namespace DCSFPTests.Serialization {
    public class OSCommandBindingFarmingPanel_SerializeTests {
        [Fact]
        public static void OSCommandBindingFarmingPanel_ShouldBeSerializable() {
            OSCommandBindingFarmingPanel s = GetObject();

            string serializedObj = JsonConvert.SerializeObject(s, Formatting.Indented, JSonSettings.JsonDefaultSettings);
            OSCommandBindingFarmingPanel d = JsonConvert.DeserializeObject<OSCommandBindingFarmingPanel>(serializedObj, JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, d.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, d.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);

            //Ignored :
            //HasSequence
            //IsSequenced

            RepositorySerialized repo = new();
            //Save sample file in project (use it only once)
            //repo.SaveSerializedObjectToFile(s.GetType(), serializedObj);

            OSCommandBindingFarmingPanel deseralizedObjFromFile = JsonConvert.DeserializeObject<OSCommandBindingFarmingPanel>(repo.GetSerializedObjectString(s.GetType()), JSonSettings.JsonDefaultSettings);

            Assert.Equal(s.FarmingPanelKey, deseralizedObjFromFile.FarmingPanelKey);
            Assert.Equal(s.WhenTurnedOn, deseralizedObjFromFile.WhenTurnedOn);
            DeepAssert.Equal(s.OSCommandObject, d.OSCommandObject);
        }

        private static OSCommandBindingFarmingPanel GetObject(int instanceNbr = 1) {
            return new()
            {
                FarmingPanelKey = KeyBindingFarmingPanel_SerializeTests.GetFarmingPanelMKKeysFromInstance(instanceNbr+7),
                WhenTurnedOn = true,
                OSCommandObject = OSCommand_SerializeTests.GetObject(instanceNbr+2)
            };
        }
    }
}
