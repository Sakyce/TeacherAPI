using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using System.Collections;
using System.Linq;
using System.Reflection;
using TeacherAPI;
using TeacherAPI.utils;
using UnityEngine;

namespace TeacherExtension.Foxo
{
    public class Foxo : Teacher
    {
        public static AssetManager sprites = new AssetManager();
        public static AssetManager audios = new AssetManager();
        public PlayerManager target;
        public bool forceWrath = false;

        // Foxo specifically uses a CustomSpriteAnimator
        public new CustomSpriteAnimator animator;

        public static void LoadAssets()
        {
            var PIXELS_PER_UNIT = 30f;
            sprites.Add(
                "Wave",
                TeacherPlugin
                    .TexturesFromMod(FoxoPlugin.Instance, "wave/Foxo_Wave{0:0000}.png", (0, 49))
                    .ToSprites(PIXELS_PER_UNIT)
            );
            sprites.Add(
                "Slap",
                TeacherPlugin
                    .TexturesFromMod(FoxoPlugin.Instance, "slap{0}.png", (1, 4))
                    .ToSprites(PIXELS_PER_UNIT)
            );
            sprites.Add(
                "Sprayed",
                TeacherPlugin
                    .TexturesFromMod(FoxoPlugin.Instance, "spray{0}.png", (1, 2))
                    .ToSprites(PIXELS_PER_UNIT)
            );
            sprites.Add(
                "Wrath",
                TeacherPlugin
                    .TexturesFromMod(FoxoPlugin.Instance, "wrath{0}.png", (1, 3))
                    .ToSprites(PIXELS_PER_UNIT)
            );
            sprites.Add(
                "Stare",
                AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(FoxoPlugin.Instance, "stare.png"), PIXELS_PER_UNIT)
            );
            sprites.Add(
                "Notebook",
                TeacherPlugin.TexturesFromMod(FoxoPlugin.Instance, "*.png", "comics").ToSprites(20f)
            );

            // Shortcut functions
            AudioClip Clip(string path) => AssetLoader.AudioClipFromMod(FoxoPlugin.Instance, "audio", path);
            SoundObject NoSubtitle(AudioClip audio, SoundType type)
            {
                var snd = ObjectCreators.CreateSoundObject(audio, "", type, Color.white);
                snd.subtitle = false;
                return snd;
            };

            audios.Add("boing", ObjectCreators.CreateSoundObject(Clip("boing.wav"), "* Boing! *", SoundType.Effect, Color.yellow));
            audios.Add("ding", ObjectCreators.CreateSoundObject(Clip("ding.wav"), "* Ding! *", SoundType.Effect, Color.yellow));
            audios.Add("school", NoSubtitle(Clip("school2.wav"), SoundType.Music));
            audios.Add("hellothere", ObjectCreators.CreateSoundObject(Clip("hellothere.wav"), "Hello there! Welcome to my Fun Schoolhouse.", SoundType.Voice, Color.yellow));
            audios.Add("slap", ObjectCreators.CreateSoundObject(Clip("slap.wav"), "* Slap! *", SoundType.Effect, Color.yellow));
            audios.Add("slap2", ObjectCreators.CreateSoundObject(Clip("slap2.wav"), "...", SoundType.Effect, Color.yellow));
            audios.Add("scare", NoSubtitle(Clip("scare.wav"), SoundType.Effect));
            audios.Add("scream", ObjectCreators.CreateSoundObject(Clip("scream.wav"), "micheal p scream", SoundType.Voice, Color.yellow));
            audios.Add("wrath", NoSubtitle(Clip("wrath.wav"), SoundType.Music));
            audios.Add("fear", NoSubtitle(Clip("fear.wav"), SoundType.Effect));

            audios.Add("praise", new SoundObject[] {
                                ObjectCreators.CreateSoundObject(Clip("praise1.wav"), "Great job, that's great!", SoundType.Voice, Color.yellow),
                                ObjectCreators.CreateSoundObject(Clip("praise2.wav"), "I think you are smarter than me!", SoundType.Voice, Color.yellow),
                        });
        }
        public override void Initialize()
        {
            base.Initialize();

            // Appearance and sound
            {
                var waveSprites = sprites.Get<Sprite[]>("Wave");
                var slapSprites = sprites.Get<Sprite[]>("Slap");
                var wrathSprites = sprites.Get<Sprite[]>("Wrath");
                animator.animations.Add("Wave", new CustomAnimation<Sprite>(waveSprites, 3f));
                animator.animations.Add("Happy", new CustomAnimation<Sprite>(new Sprite[] { waveSprites[waveSprites.Length - 1] }, 1f));
                animator.animations.Add("Stare", new CustomAnimation<Sprite>(new Sprite[] { sprites.Get<Sprite>("Stare") }, 1f));

                animator.animations.Add("Slap", new CustomAnimation<Sprite>(slapSprites, 1f));
                animator.animations.Add("SlapIdle", new CustomAnimation<Sprite>(new Sprite[] { slapSprites[slapSprites.Length - 1] }, 1f));

                animator.animations.Add("WrathIdle", new CustomAnimation<Sprite>(new Sprite[] { wrathSprites[0] }, 1f));
                animator.animations.Add("Wrath", new CustomAnimation<Sprite>(wrathSprites.Reverse().ToArray(), 0.3f));
                navigator.Entity.SetHeight(6.5f);
                AddLoseSound(audios.Get<SoundObject>("scare"), 1);
            }

            // Foxo specific
            {
                target = ec.Players[0];
                baseAnger += 1;
                baseSpeed *= 0.6f;
            }

            // Random events
            ReplaceEventText<RulerEvent>("Uh oh, Foxo broke his ruler. This is not good.");
        }
        public override TeacherState GetAngryState() => forceWrath ? (Foxo_StateBase)(new Foxo_Wrath(this)) : new Foxo_Chase(this);
        public override TeacherState GetHappyState() => forceWrath ? (Foxo_StateBase)(new Foxo_WrathHappy(this)) : new Foxo_Happy(this);
        public override string GetNotebooksText(string amount) => $"{amount} Foxo Comics";
        public override WeightedTeacherNotebook GetTeacherNotebookWeight()
            => new WeightedTeacherNotebook(this).Weight(100).Sprite(sprites.Get<Sprite[]>("Notebook"));
        // Only play visual/audio effects, doesn't actually moves
        public new void SlapNormal()
        {

            animator.SetDefaultAnimation("SlapIdle", 1f);
            animator.Play("Slap", 1f);
            SlapRumble();
            AudMan.PlaySingle(audios.Get<SoundObject>("slap"));
        }
        public new void SlapBroken()
        {
            animator.SetDefaultAnimation("WrathIdle", 1f);
            animator.Play("Wrath", 1f);
            SlapRumble();
            AudMan.PlaySingle(audios.Get<SoundObject>("slap2"));
        }

        // Ruler related events
        protected override void OnRulerBroken()
        {
            base.OnRulerBroken();
            extraAnger += 3;
            behaviorStateMachine.ChangeState(new Foxo_Wrath(this));
        }
        protected override void OnRulerRestored()
        {
            base.OnRulerRestored();
            behaviorStateMachine.ChangeState(new Foxo_Chase(this));
        }

        // Foxo specific
        public void TeleportToNearestDoor()
        {
            var playerPos = ec.players[0].transform.position;
            Door nearestDoor = null;
            var nearest = float.PositiveInfinity;

            // Get nearest door
            foreach (var tile in ec.AllCells())
            {
                foreach (var door in tile.doors)
                {
                    var distance = (door.transform.position - playerPos).magnitude;
                    if (distance <= nearest)
                    {
                        nearestDoor = door;
                        nearest = distance;
                    }
                }
            }

            if (nearestDoor == null)
            {
                Debug.LogWarning("No nearest door found for Foxo");
                return;
            }

            // Get most far side
            Vector3 teleportPosition;
            if ((nearestDoor.aTile.tile.transform.position - playerPos).magnitude < (nearestDoor.bTile.tile.transform.position - playerPos).magnitude)
            {
                teleportPosition = nearestDoor.bTile.tile.transform.position;
            }
            else
            {
                teleportPosition = nearestDoor.aTile.tile.transform.position;
            }
            transform.position = teleportPosition + Vector3.up * 5f;
        }
        public override void VirtualUpdate()
        {
            base.VirtualUpdate();
            if (!target.plm.running)
                target.plm.AddStamina(target.plm.staminaDrop * 0.8f * Time.deltaTime * target.PlayerTimeScale, true);
        }

    }
    public class Foxo_StateBase : TeacherState
    {
        public Foxo_StateBase(Foxo foxo) : base(foxo)
        {
            this.foxo = foxo;
        }
        protected virtual void ActivateSlapAnimation() { }
        protected Foxo foxo;
    }
    public class Foxo_Happy : Foxo_StateBase
    {
        public Foxo_Happy(Foxo foxo) : base(foxo) { }
        public override void Enter()
        {
            base.Enter();
            foxo.animator.Play("Wave", 1f);
            foxo.animator.SetDefaultAnimation("Happy", 1f);
            foxo.audMan.PlaySingle(Foxo.audios.Get<SoundObject>("hellothere"));
            foxo.navigator.SetSpeed(0f);
            ChangeNavigationState(new NavigationState_DoNothing(foxo, 32));

            foxo.ReplaceMusic(Foxo.audios.Get<SoundObject>("school"));
        }

        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            if (foxo.IsHelping())
            {
                foxo.ActivateSpoopMode();
                foxo.behaviorStateMachine.ChangeState(new Foxo_Chase(foxo));
                return;
            }
            foxo.behaviorStateMachine.ChangeState(new Foxo_Scary(foxo));
        }
    }
    public class Foxo_Scary : Foxo_StateBase
    {
        public Foxo_Scary(Foxo foxo) : base(foxo) { }
        public override void Enter()
        {
            base.Enter();
            foxo.animator.SetDefaultAnimation("Stare", 1f);
            foxo.animator.Play("Stare", 1f);

            // Stop playing songs and be scary for once!!
            Singleton<MusicManager>.Instance.StopMidi();
            foxo.ec.lightMode = LightMode.Greatest;
            foxo.ec.standardDarkLevel = Color.black;
            foxo.ec.audMan.FlushQueue(true);
            foxo.ec.audMan.PlaySingle(Foxo.audios.Get<SoundObject>("fear"));
            foxo.ec.FlickerLights(true);
            foxo.TeleportToNearestDoor();

            foxo.StartCoroutine(GetMad());
        }
        private IEnumerator GetMad()
        {
            yield return new WaitForSeconds(13f);
            foxo.ec.FlickerLights(false);
            foxo.ec.audMan.PlaySingle(Foxo.audios.Get<SoundObject>("ding"));
            foxo.ActivateSpoopMode();
            foxo.behaviorStateMachine.ChangeState(new Foxo_Chase(foxo));
            yield break;
        }
    }
    public class Foxo_Chase : Foxo_StateBase
    {
        protected float delayTimer;
        public Foxo_Chase(Foxo foxo) : base(foxo) { }
        public override void OnStateTriggerStay(Collider other)
        {
            if (foxo.IsTouchingPlayer(other))
            {
                foxo.CaughtPlayer(foxo.target);
            }
        }
        public override void GoodMathMachineAnswer()
        {
            base.GoodMathMachineAnswer();
            foxo.behaviorStateMachine.ChangeState(new Foxo_Praise(foxo, this));
        }
        public override void Enter()
        {
            base.Enter();
            foxo.animator.SetDefaultAnimation("SlapIdle", 1f);
            delayTimer = foxo.Delay;
            foxo.ResetSlapDistance();
        }
        public override void Update()
        {
            base.Update();
            foxo.UpdateSlapDistance();
            delayTimer -= Time.deltaTime * npc.TimeScale;

            if (foxo.forceWrath && GetType().Equals(typeof(Foxo_Chase)))
            {
                foxo.behaviorStateMachine.ChangeState(new Foxo_Wrath(foxo));
                return;
            }

            if (delayTimer <= 0f)
            {
                // Progressive restoration after wrath
                if (foxo.extraAnger > 0 && GetType().Equals(typeof(Foxo_Chase)))
                {
                    foxo.extraAnger -= 1;
                }

                // Foxo always know where the player is, except in special rooms
                if (foxo.target.currentRoom != null && foxo.target.currentRoom.category != RoomCategory.Special)
                {
                    ChangeNavigationState(new NavigationState_TargetPlayer(foxo, 0, foxo.target.transform.position));
                }

                foxo.Slap();
                ActivateSlapAnimation();
                delayTimer = foxo.Delay;
            }
        }
        protected override void ActivateSlapAnimation() => foxo.SlapNormal();
    }
    public class Foxo_Praise : Foxo_StateBase
    {
        public TeacherState previousState;
        private float time;

        public Foxo_Praise(Foxo foxo, TeacherState previousState) : base(foxo)
        {
            this.previousState = previousState;
            time = 4f;
        }
        public override void Enter()
        {
            base.Enter();
            foxo.audMan.PlayRandomAudio(Foxo.audios.Get<SoundObject[]>("praise"));
            foxo.animator.SetDefaultAnimation("Happy", 1f);
            foxo.animator.Play("Happy", 1f);
        }
        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime * npc.TimeScale;
            if (time <= 0) foxo.behaviorStateMachine.ChangeState(previousState);
        }
    }

    public class Foxo_WrathHappy : Foxo_StateBase
    {
        public Foxo_WrathHappy(Foxo foxo) : base(foxo) { }

        public override void Enter()
        {
            base.Enter();
            foxo.animator.SetDefaultAnimation("WrathIdle", 1f);
            foxo.navigator.SetSpeed(0f);
            foxo.spriteBase.SetActive(false);
            ChangeNavigationState(new NavigationState_DoNothing(foxo, 32));
            foxo.ReplaceMusic();
        }
        public override void Exit()
        {
            base.Exit();
            foxo.ec.lightMode = LightMode.Greatest;
            foxo.ec.standardDarkLevel = Color.black;
            foxo.spriteBase.SetActive(true);
        }
        public override void NotebookCollected(int c, int m)
        {
            base.NotebookCollected(c, m);
            foxo.ActivateSpoopMode();
            foxo.behaviorStateMachine.ChangeState(new Foxo_Wrath(foxo));
        }
    }
    public class Foxo_Wrath : Foxo_Chase
    {
        public Foxo_Wrath(Foxo foxo) : base(foxo) { }
        protected override void ActivateSlapAnimation()
        {
            // 6 Lines of code for a sound effect 💀
            if (!isBroken)
            {
                foxo.AudMan.PlaySingle(TeacherPlugin.Instance.CurrentBaldi.rulerBreak);
                isBroken = true;
            }
            foxo.SlapBroken();
        }
        public override void Enter()
        {
            base.Enter();
            foxo.ec.FlickerLights(false);
            foxo.ec.audMan.PlaySingle(Foxo.audios.Get<SoundObject>("wrath"));
        }
        public override void Exit()
        {
            base.Exit();
            foxo.ec.FlickerLights(false);
        }

        private bool isBroken = false;
    }
}
