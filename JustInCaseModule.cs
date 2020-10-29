using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace JustInCase
{
    public class JustInCaseModule : ETGModule
    {
        public override void Init()
        {
        }

        public override void Start()
        {
            Gun uppercaseR = PickupObjectDatabase.GetById(649) as Gun;
            Gun lowercaseR = PickupObjectDatabase.GetById(340) as Gun;
            if(lowercaseR != null && uppercaseR != null)
            {
                uppercaseR.quality = PickupObject.ItemQuality.D;
                uppercaseR.ForcedPositionInAmmonomicon = lowercaseR.ForcedPositionInAmmonomicon;
                EncounterTrackable uppercaseTrackable = uppercaseR.GetComponent<EncounterTrackable>();
                if(uppercaseTrackable == null)
                {
                    uppercaseTrackable = uppercaseR.gameObject.AddComponent<EncounterTrackable>();
                    if (uppercaseTrackable.journalData == null)
                    {
                        uppercaseTrackable.journalData = new JournalEntry();
                    }
                    uppercaseTrackable.journalData.SuppressKnownState = false;
                    uppercaseTrackable.journalData.SuppressInAmmonomicon = false;
                    tk2dSpriteDefinition def = uppercaseR.sprite.Collection.spriteDefinitions[uppercaseR.sprite.spriteId];
                    uppercaseTrackable.journalData.AmmonomiconSprite = def.name;
                    AddToAmmonomicon(def);
                }
                else
                {
                    if (uppercaseTrackable.journalData == null)
                    {
                        uppercaseTrackable.journalData = new JournalEntry();
                    }
                    uppercaseTrackable.journalData.SuppressKnownState = false;
                    uppercaseTrackable.journalData.SuppressInAmmonomicon = false;
                    tk2dSpriteDefinition def = uppercaseR.sprite.Collection.spriteDefinitions[uppercaseR.sprite.spriteId];
                    uppercaseTrackable.journalData.AmmonomiconSprite = def.name;
                    AddToAmmonomicon(def);
                }
                TransformGunSynergyProcessor processor = lowercaseR.GetComponents<TransformGunSynergyProcessor>().ToList().Find((TransformGunSynergyProcessor transform) => transform.SynergyToCheck == CustomSynergyType.JUST_IN_CASE);
                WeightedGameObject weightedGameObject = new WeightedGameObject
                {
                    weight = 1f,
                    additionalPrerequisites = new DungeonPrerequisite[0]
                };
                weightedGameObject.SetGameObject(uppercaseR.gameObject);
                GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.Add(weightedGameObject);
                if (processor != null)
                {
                    UnityEngine.Object.Destroy(processor);
                }
                DualWieldSynergyProcessor dualWieldUppercase = uppercaseR.gameObject.AddComponent<DualWieldSynergyProcessor>();
                dualWieldUppercase.SynergyToCheck = CustomSynergyType.JUST_IN_CASE;
                dualWieldUppercase.PartnerGunID = 340;
                DualWieldSynergyProcessor dualWieldLowercase = lowercaseR.gameObject.AddComponent<DualWieldSynergyProcessor>();
                dualWieldLowercase.SynergyToCheck = CustomSynergyType.JUST_IN_CASE;
                dualWieldLowercase.PartnerGunID = 649;
                AdvancedSynergyEntry entry = GameManager.Instance.SynergyManager.synergies.ToList().Find((AdvancedSynergyEntry synergy) => synergy.bonusSynergies.Contains(CustomSynergyType.JUST_IN_CASE) && synergy.NameKey == "#JUSTINCASE");
                if (entry != null)
                {
                    if (entry.MandatoryItemIDs != null && entry.MandatoryItemIDs.Contains(108))
                    {
                        for (int i = 0; i < entry.MandatoryItemIDs.Count; i++)
                        {
                            if (entry.MandatoryItemIDs[i] == 108)
                            {
                                entry.MandatoryItemIDs.RemoveAt(i);
                                i--;
                                entry.MandatoryGunIDs.Add(649);
                            }
                        }
                    }
                    if (entry.OptionalItemIDs != null && entry.OptionalItemIDs.Contains(108))
                    {
                        for (int i = 0; i < entry.OptionalItemIDs.Count; i++)
                        {
                            if (entry.OptionalItemIDs[i] == 108)
                            {
                                entry.OptionalItemIDs.RemoveAt(i);
                                i--;
                                entry.OptionalGunIDs.Add(649);
                            }
                        }
                    }
                }
                else
                {
                    AdvancedSynergyEntry newEntry = new AdvancedSynergyEntry
                    {
                        IgnoreLichEyeBullets = false,
                        ActiveWhenGunUnequipped = false,
                        bonusSynergies = new List<CustomSynergyType> { CustomSynergyType.JUST_IN_CASE },
                        MandatoryGunIDs = new List<int> { 340, 649 },
                        MandatoryItemIDs = new List<int>(),
                        NameKey = "#JUSTINCASE",
                        NumberObjectsRequired = 2,
                        OptionalGunIDs = new List<int>(),
                        OptionalItemIDs = new List<int>(),
                        RequiresAtLeastOneGunAndOneItem = false,
                        statModifiers = new List<StatModifier>(),
                        SuppressVFX = false
                    };
                    GameManager.Instance.SynergyManager.synergies = GameManager.Instance.SynergyManager.synergies.Concat(new AdvancedSynergyEntry[] { newEntry }).ToArray();
                }
            }
            ETGModConsole.Log("Just In Case started successfully.");
        }

        public static int AddToAmmonomicon(tk2dSpriteDefinition spriteDefinition)
        {
            tk2dSpriteDefinition copyDef = CopyDefinitionFrom(spriteDefinition);
            Shader ammonomiconShader = ShaderCache.Acquire("tk2d/CutoutVertexColorTilted");
            if(copyDef.material != null)
            {
                copyDef.material.shader = ammonomiconShader;
            }
            if(copyDef.materialInst != null)
            {
                copyDef.materialInst.shader = ammonomiconShader;
            }
            return AddSpriteToCollection(spriteDefinition, ammonomiconCollection);
        }

        public static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
        {
            //Add definition to collection
            var defs = collection.spriteDefinitions;
            var newDefs = defs.Concat(new tk2dSpriteDefinition[] { spriteDefinition }).ToArray();
            collection.spriteDefinitions = newDefs;

            //Reset lookup dictionary
            FieldInfo f = typeof(tk2dSpriteCollectionData).GetField("spriteNameLookupDict", BindingFlags.Instance | BindingFlags.NonPublic);
            f.SetValue(collection, null);  //Set dictionary to null
            collection.InitDictionary(); //InitDictionary only runs if the dictionary is null
            return newDefs.Length - 1;
        }

        public override void Exit()
        {
        }

        public static tk2dSpriteDefinition CopyDefinitionFrom(tk2dSpriteDefinition other)
        {
            tk2dSpriteDefinition result = new tk2dSpriteDefinition
            {
                boundsDataCenter = new Vector3
                {
                    x = other.boundsDataCenter.x,
                    y = other.boundsDataCenter.y,
                    z = other.boundsDataCenter.z
                },
                boundsDataExtents = new Vector3
                {
                    x = other.boundsDataExtents.x,
                    y = other.boundsDataExtents.y,
                    z = other.boundsDataExtents.z
                },
                colliderConvex = other.colliderConvex,
                colliderSmoothSphereCollisions = other.colliderSmoothSphereCollisions,
                colliderType = other.colliderType,
                colliderVertices = other.colliderVertices,
                collisionLayer = other.collisionLayer,
                complexGeometry = other.complexGeometry,
                extractRegion = other.extractRegion,
                flipped = other.flipped,
                indices = other.indices,
                materialId = other.materialId,
                metadata = other.metadata,
                name = other.name,
                normals = other.normals,
                physicsEngine = other.physicsEngine,
                position0 = new Vector3
                {
                    x = other.position0.x,
                    y = other.position0.y,
                    z = other.position0.z
                },
                position1 = new Vector3
                {
                    x = other.position1.x,
                    y = other.position1.y,
                    z = other.position1.z
                },
                position2 = new Vector3
                {
                    x = other.position2.x,
                    y = other.position2.y,
                    z = other.position2.z
                },
                position3 = new Vector3
                {
                    x = other.position3.x,
                    y = other.position3.y,
                    z = other.position3.z
                },
                regionH = other.regionH,
                regionW = other.regionW,
                regionX = other.regionX,
                regionY = other.regionY,
                tangents = other.tangents,
                texelSize = new Vector2
                {
                    x = other.texelSize.x,
                    y = other.texelSize.y
                },
                untrimmedBoundsDataCenter = new Vector3
                {
                    x = other.untrimmedBoundsDataCenter.x,
                    y = other.untrimmedBoundsDataCenter.y,
                    z = other.untrimmedBoundsDataCenter.z
                },
                untrimmedBoundsDataExtents = new Vector3
                {
                    x = other.untrimmedBoundsDataExtents.x,
                    y = other.untrimmedBoundsDataExtents.y,
                    z = other.untrimmedBoundsDataExtents.z
                }
            };
            if(other.material != null)
            {
                result.material = new Material(other.material);
            }
            else
            {
                result.material = null;
            }
            if(other.materialInst != null)
            {
                result.materialInst = new Material(other.materialInst);
            }
            else
            {
                result.materialInst = null;
            }
            if (other.uvs != null)
            {
                List<Vector2> uvs = new List<Vector2>();
                foreach (Vector2 vector in other.uvs)
                {
                    uvs.Add(new Vector2
                    {
                        x = vector.x,
                        y = vector.y
                    });
                }
                result.uvs = uvs.ToArray();
            }
            else
            {
                result.uvs = null;
            }
            if (other.colliderVertices != null)
            {
                List<Vector3> colliderVertices = new List<Vector3>();
                foreach (Vector3 vector in other.colliderVertices)
                {
                    colliderVertices.Add(new Vector3
                    {
                        x = vector.x,
                        y = vector.y,
                        z = vector.z
                    });
                }
                result.colliderVertices = colliderVertices.ToArray();
            }
            else
            {
                result.colliderVertices = null;
            }
            return result;
        }

        public static tk2dSpriteCollectionData ammonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
    }
}
