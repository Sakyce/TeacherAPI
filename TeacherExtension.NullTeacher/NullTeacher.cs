using MTM101BaldAPI.Components;
using System.Collections.Generic;
using System.Linq;
using TeacherAPI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NullTeacher
{
    public class NullTeacher : Teacher
    {
        public new CustomSpriteAnimator animator;

        public HashSet<NullPhrase> saidPhrases = new HashSet<NullPhrase>();
        public HashSet<Items> usefulItems = new HashSet<Items>();
        public List<NullPhrase> genericPhrases = new List<NullPhrase>();
        public List<Cell> lightsToChange = new List<Cell>();
        public bool hidden = false;
        public float flickerDelay = 1;

        public float genericSpeechDelay = 30f;
        public float timeSinceNullHasSeenPlayer = 0f;
        public float gameTime = 0f;
        public static float timeSinceExcitingThing = 0f;

        public Cell previousCell;
        public Cell currentCell;

        public NullTeacher()
        {
            disableNpcs = true;
            caughtOffset = new Vector3(0, 0, 0);
        }

        public override void GetAngry(float value)
        {
            base.GetAngry(value * 0.5f);
        }

        public override void Initialize()
        {
            base.Initialize();

            genericPhrases.Add(NullPhrase.Bored);
            genericPhrases.Add(NullPhrase.Scary);
            genericPhrases.Add(NullPhrase.Stop);
            genericPhrases.Add(NullPhrase.Where);

            foreach (var itemtype in new Items[] { Items.DoorLock, Items.Wd40, Items.PortalPoster, Items.ChalkEraser, Items.Bsoda, Items.GrapplingHook, Items.ZestyBar, Items.Teleporter })
            {
                usefulItems.Add(itemtype);
            }

            baseSpeed = 6;
            baseAnger = 0.05f;
            extraAngerDrain = 0.1f;

            animator.animations.Add("Normal", new CustomAnimation<Sprite>(new Sprite[] { NullAssets.nullsprite }, 1f));
            animator.animations.Add("Baldloon", new CustomAnimation<Sprite>(new Sprite[] { NullAssets.baldloon }, 1f));
            animator.SetDefaultAnimation(NullConfiguration.ReplaceNullWithBaldloon.Value ? "Baldloon" : "Normal", 1f);
            looker.layerMask = 98305; // can see windows at this layer

            navigator.Entity.SetHeight(5f);

            AddLoseSound(
                CoreGameManager.Instance.lives < 1 && CoreGameManager.Instance.extraLives < 1
                ? NullAssets.phrases[NullPhrase.Haha]
                : NullAssets.lose, 1);

            ReplaceEventText<RulerEvent>("What do you mean Null broke his ruler?!");

            foreach (var light in ec.lights)
            {
                if (light.lightStrength > 1)
                {
                    lightsToChange.Add(light);
                }
            }

            var randomEvent = Resources.FindObjectsOfTypeAll<PartyEvent>().First();
            foreach (var room in ec.rooms)
            {
                if (room.category.Equals(RoomCategory.Faculty) || room.category.Equals(RoomCategory.Hall) || room.category.Equals(RoomCategory.Office))
                {
                    for (var i = 0; i < Random.Range(4, 6); i++)
                    {
                        Instantiate(randomEvent.balloon[Random.Range(0, randomEvent.balloon.Length)]).Initialize(room);
                    }
                }
            }
            ec.lightMode = LightMode.Greatest;
            ec.standardDarkLevel = new Color(0.15f, 0.025f, 0.15f);
            ec.FlickerLights(false); // Doing this will update the lights

            if (!IsHelping())
            {
                foreach (var activity in ec.activities)
                {
                    activity.Corrupt(true);
                }
            }
            spriteBase.SetActive(false);
        }
        public override TeacherState GetAngryState() => new Null_Chase(this);
        public override TeacherState GetHappyState() => new Null_Happy(this);
        public override string GetNotebooksText(string amount) => $"{amount} Noteboos";
        public override WeightedTeacherNotebook GetTeacherNotebookWeight() => new WeightedTeacherNotebook(this).Weight(100);
        public override void VirtualUpdate()
        {
            base.VirtualUpdate();

            if (ec.CellFromPosition(IntVector2.GetGridPosition(transform.position)) != currentCell)
            {
                previousCell = currentCell;
                currentCell = ec.CellFromPosition(IntVector2.GetGridPosition(transform.position));
            }

            var dt = Time.deltaTime * ec.NpcTimeScale;
            gameTime += dt;
            timeSinceNullHasSeenPlayer += dt;
            timeSinceExcitingThing += dt;
        }

        public void OpenDoors()
        {
            if (currentCell != null && currentCell.doorHere)
            {
                currentCell.doors.ForEach(d => d.OpenTimed(0.5f, false));
                SpeechCheck(NullPhrase.Hide, 0.01f);
            }
        }

        public void FlickerLights()
        {
            flickerDelay -= Time.deltaTime * this.ec.EnvironmentTimeScale;

            if (!hidden)
            {
                // Blindly copied from NullNPC
                foreach (var light in lightsToChange)
                {
                    var dist = Vector3.Distance(transform.position, light.TileTransform.position);
                    var num = (dist - 30f) / 70f;
                    if (dist <= 30f)
                    {
                        if (light.lightOn) ec.SetLight(false, light);
                    }
                    else if (dist <= 100)
                    {
                        if (flickerDelay <= 0f && Random.Range(0f, 1f) <= 0.1f)
                        {
                            if (!light.lightOn)
                            {
                                if (Random.Range(0f, 1f) <= num) ec.SetLight(true, light);
                            }
                            else if (Random.Range(0f, 1f) >= num) ec.SetLight(false, light);
                        }
                    }
                    else if (light.lightOn)
                    {
                        ec.SetLight(true, light);
                    }
                }
            }
            if (flickerDelay <= 0)
            {
                flickerDelay = 0.1f;
            }
        }

        public void CheckSpeeches()
        {
            genericSpeechDelay -= Time.deltaTime * ec.NpcTimeScale;
            if (genericSpeechDelay <= 0)
            {
                SpeechCheck(NullPhrase.Generic, 0.01f);
                genericSpeechDelay = 30f;
            }

            var plr = CoreGameManager.Instance.GetPlayer(0);
            var hasItem = false;
            foreach (var item in plr.itm.items)
            {
                if (usefulItems.Contains(item.itemType))
                {
                    hasItem = true;
                }
            }
            if (timeSinceNullHasSeenPlayer <= 10 && !hasItem && plr.plm.stamina <= plr.plm.staminaMax / 10 && anger >= 4f)
            {
                SpeechCheck(NullPhrase.Nothing, 0.2f);
            }

            // I don't understand this part AT ALL
            if (currentCell != null && previousCell != null)
            {
                int navBin = currentCell.NavBin;
                for (int i = 0; i < 4; i++)
                {
                    if ((navBin & (1 << i)) == 0) currentCell.SilentBlock((Direction)i, true);
                }
                if (!ec.CheckPath(previousCell, ec.CellFromPosition(IntVector2.GetGridPosition(plr.transform.position)), PathType.Nav))
                {
                    SpeechCheck(NullPhrase.Enough, 0.04f);
                }
                for (int j = 0; j < 4; j++)
                {
                    if ((navBin & (1 << j)) == 0) currentCell.SilentBlock((Direction)j, false);
                }
            }
        }

        private void PlayGenericPhrase(NullPhrase phrase)
        {
            genericPhrases.Remove(phrase);
            try { audMan.QueueAudio(NullAssets.phrases[phrase]); }
            catch (KeyNotFoundException) { Debug.LogWarning($"No sound called {phrase} exists for Null yet"); }
        }
        private void PlayPhrase(NullPhrase phrase)
        {
            saidPhrases.Add(phrase);
            try { audMan.QueueAudio(NullAssets.phrases[phrase]); }
            catch (KeyNotFoundException) { Debug.LogWarning($"No sound called {phrase} exists for Null yet"); }
        }

        public void SpeechCheck(NullPhrase phrase, float chance)
        {
            if (NullConfiguration.ReplaceNullWithBaldloon.Value) return;

            switch (phrase)
            {
                case NullPhrase.Nothing:
                case NullPhrase.Enough:
                case NullPhrase.Hide:
                case NullPhrase.Bored:
                    if (!saidPhrases.Contains(phrase) && Random.Range(0f, 1f) <= chance && !audMan.AnyAudioIsPlaying)
                    {
                        PlayPhrase(phrase);
                    }
                    break;

                // TBD
                case NullPhrase.Generic:
                    if (Random.Range(0f, 1f) <= chance)
                    {
                        var phrases = new List<NullPhrase>(genericPhrases);
                        while (phrases.Count > 0)
                        {
                            // Select a random null phrase
                            var phrase2 = phrases[Random.Range(0, phrases.Count)];

                            switch (phrase2)
                            {
                                case NullPhrase.Where:
                                    if (timeSinceNullHasSeenPlayer >= 20f && gameTime >= 60f) PlayGenericPhrase(phrase2);
                                    break;
                                case NullPhrase.Bored:
                                    if (timeSinceExcitingThing > 30f && gameTime >= 100f) PlayGenericPhrase(phrase2);
                                    break;
                                case NullPhrase.Scary:
                                    if (gameTime > 120f && !looker.PlayerInSight()) PlayGenericPhrase(phrase2);
                                    break;
                                default:
                                    PlayGenericPhrase(phrase2);
                                    break;
                            }

                            phrases.Remove(phrase2);
                        }
                        return;
                    }
                    break;

                default: break;
            }
        }
    }

    public class Null_StateBase : TeacherState
    {
        public Null_StateBase(NullTeacher ohno) : base(ohno)
        {
            this.ohno = ohno;
        }

        public override void Hear(Vector3 position, int value)
        {
            base.Hear(position, value);
            ohno.Hear(position, value, true);
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            ohno.ClearSoundLocations();
            ohno.Hear(player.transform.position, 127, false);
            ohno.timeSinceNullHasSeenPlayer = 0f;
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ohno.UpdateSoundTarget();
        }
        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            ohno.Hear(CoreGameManager.Instance.GetPlayer(0).transform.position, 127, true);
        }

        protected NullTeacher ohno;
    }

    public class Null_Happy : Null_StateBase
    {
        public Null_Happy(NullTeacher ohno) : base(ohno)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ohno.ReplaceMusic(NullAssets.ambient);
        }

        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            ohno.behaviorStateMachine.ChangeState(new Null_Chase(ohno));
            ohno.ActivateSpoopMode();
        }
    }

    public class Null_Chase : Null_StateBase
    {
        private float timer;
        private Material noGllitchMat;
        public Null_Chase(NullTeacher Null) : base(Null)
        {
        }

        public override void Enter()
        {
            base.Enter();
            noGllitchMat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(x => x.name == "SpriteStandard_Billboard_NoGlitch");
            if (ohno.IsHelping())
            {
                ohno.transform.position = ohno.ec.elevators[0].transform.position;
            }
            timer = ohno.Delay;
            ohno.ResetSlapDistance();
            ohno.spriteBase.SetActive(true);
        }

        public override void Update()
        {
            base.Update();
            ohno.OpenDoors();
            ohno.CheckSpeeches();
            ohno.FlickerLights();

            ohno.UpdateSlapDistance();
            timer -= Time.deltaTime * npc.TimeScale;
            if (timer <= 0f)
            {
                ohno.Slap();
                timer = ohno.Delay;
            }
        }

        public override void OnStateTriggerStay(Collider other)
        {
            if (teacher.IsTouchingPlayer(other))
            {
                // KILL
                ohno.AudMan.audioDevice.ignoreListenerPause = true;
                ohno.SpeechCheck(NullPhrase.Haha, 1f);
                ohno.ec.SetAllLights(true);
                ohno.spriteRenderer[0].material = noGllitchMat;
                Shader.SetGlobalColor("_SkyboxColor", Color.black);

                teacher.CaughtPlayer(other.GetComponent<PlayerManager>());
                ohno.behaviorStateMachine.ChangeState(new Null_Caught(ohno));
            }
            else if (ohno.Navigator.passableObstacles.Contains(PassableObstacle.Window) && other.CompareTag("Window"))
            {
                // WINDOW BREAK
                other.GetComponent<Window>().Break(false);
                ohno.SpeechCheck(NullPhrase.Hide, 0.10f);
            }
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            if (!ohno.Navigator.passableObstacles.Contains(PassableObstacle.Window))
            {
                Debug.Log("Added passable obstacle");
                ohno.Navigator.passableObstacles.Add(PassableObstacle.Window);
                ohno.Navigator.CheckPath();
            }
        }

        public override void DestinationEmpty()
        {
            if (ohno.Navigator.passableObstacles.Contains(PassableObstacle.Window))
            {
                ohno.Navigator.passableObstacles.Clear();
            }
            if (ohno.hidden)
            {
                ohno.spriteBase.SetActive(false);
            }
            base.DestinationEmpty();
        }
    }

    public class Null_Caught : Null_StateBase
    {
        public Null_Caught(NullTeacher ohno) : base(ohno) { }
    }


    public enum NullPhrase
    {
        Bored,
        Enough,
        Haha,
        Hide,
        Nothing,
        Scary,
        Stop,
        Where,
        Generic
    }
}
