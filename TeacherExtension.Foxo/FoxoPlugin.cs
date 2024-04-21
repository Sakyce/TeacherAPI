using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BepInEx.BepInDependency;
using TeacherAPI;
using TeacherAPI.utils;
using System.IO;

namespace TeacherExtension.Foxo
{
    [BepInPlugin("sakyce.baldiplus.teacherextension.foxo", "Foxo Teacher for MoreTeachers", "1.0.0.0")]
    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    public class FoxoPlugin : BaseUnityPlugin
    {
        public static FoxoPlugin Instance { get; private set; }
        public Foxo Foxo { get;  private set; }
        public Foxo DarkFoxo { get; private set; }

        internal void Awake()
        {
            new Harmony("sakyce.baldiplus.teacherextension.foxo").PatchAllConditionals();
            Instance = this;
            FoxoConfiguration.Setup();
            LoadingEvents.RegisterOnAssetsLoaded(OnAssetsLoaded, false);
        }

        private Foxo NewFoxo(string name)
        {
            var newFoxo = ObjectCreators.CreateNPC<Foxo>(
                name,
                EnumExtensions.ExtendEnum<Character>(name),
                ObjectCreators.CreatePosterObject(new Texture2D[] { AssetLoader.TextureFromMod(this, "poster.png") })
            );
            newFoxo.audMan = newFoxo.GetComponent<AudioManager>();

            // Adds a custom animator
            CustomSpriteAnimator animator = newFoxo.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = newFoxo.spriteRenderer[0];
            newFoxo.animator = animator;

            NPCMetaStorage.Instance.Add(new NPCMetadata(Info, new NPC[] { newFoxo }, name, NPCFlags.Standard));
            return newFoxo;
        }

        private void OnAssetsLoaded()
        {
            Foxo.LoadAssets();

            // Create and Register Foxo and DarkFoxo
            {
                Foxo = NewFoxo("Foxo");
                DarkFoxo = NewFoxo("WrathFoxo");
                DarkFoxo.forceWrath = true;

                TeacherPlugin.RegisterTeacher(Foxo);
                TeacherPlugin.RegisterTeacher(DarkFoxo);
            }

            GeneratorManagement.Register(this, GenerationModType.Addend, EditGenerator);
        }

        private void EditGenerator(string floorName, int floorNumber, LevelObject floorObject)
        {
            // It is good practice to check if the level starts with F to make sure to not clash with other mods.
            // INF stands for Infinite Floor
            if (floorName.StartsWith("F") || floorName.StartsWith("END") || floorName.Equals("INF"))
            {
                floorObject.AddPotentialTeacher(Foxo, FoxoConfiguration.Weight.Value);
                print($"Added Foxo to {floorName} (Floor {floorNumber})");
            }
        }
    }
}
