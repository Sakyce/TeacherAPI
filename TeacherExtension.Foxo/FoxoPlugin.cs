using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using System.Linq;
using TeacherAPI;
using UnityEngine;
using static BepInEx.BepInDependency;

namespace TeacherExtension.Foxo
{
    [BepInPlugin("sakyce.baldiplus.teacherextension.foxo", "Foxo Teacher for MoreTeachers", "1.0.0.0")]
    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    public class FoxoPlugin : BaseUnityPlugin
    {
        public static FoxoPlugin Instance { get; private set; }
        public Foxo Foxo { get; private set; }
        public Foxo DarkFoxo { get; private set; }

        internal void Awake()
        {
            new Harmony("sakyce.baldiplus.teacherextension.foxo").PatchAllConditionals();
            Instance = this;
            FoxoConfiguration.Setup();
            TeacherPlugin.RequiresAssetsFolder(this); // Critical!!!
            LoadingEvents.RegisterOnAssetsLoaded(Info, OnAssetsLoaded, false);
        }

        private Foxo NewFoxo(string name)
        {
            var newFoxo = new NPCBuilder<Foxo>(Info)
                .SetName(name)
                .SetEnum(name)
                .SetPoster(ObjectCreators.CreatePosterObject(new Texture2D[] { AssetLoader.TextureFromMod(this, "poster.png") }))
                .AddLooker()
                .AddTrigger()
                .SetMetaTags(new string[] { "Teacher" })
                .Build();
            newFoxo.audMan = newFoxo.GetComponent<AudioManager>();

            // Adds a custom animator
            CustomSpriteAnimator animator = newFoxo.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = newFoxo.spriteRenderer[0];
            newFoxo.animator = animator;
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
