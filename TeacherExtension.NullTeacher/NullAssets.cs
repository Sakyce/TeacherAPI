using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using UnityEngine;

namespace NullTeacher
{
    // AssetManager could be used, but not too sure of its efficiency for this use case.
    public static class NullAssets
    {
        private static SoundObject Quote(string path, string subtitle)
        {
            return ObjectCreators.CreateSoundObject(
                AssetLoader.AudioClipFromMod(NullTeacherPlugin.Instance, "quotes", path),
                subtitle,
                SoundType.Voice,
                Color.white
            );
        }
        internal static void Load()
        {
            phrases.Add(NullPhrase.Enough, Quote("enough.wav", "I've had enough! I've had enough."));
            phrases.Add(NullPhrase.Haha, Quote("haha.wav", "Ha haa!"));
            phrases.Add(NullPhrase.Hide, Quote("nowhereyoucanhide.wav", "There's nowhere you can hide."));
            phrases.Add(NullPhrase.Bored, Quote("bored.wav", "Are you bored yet?"));

            var where = Quote("whereveryouare.wav", "Hey! Wherever you are I'm gonna find you,");
            where.additionalKeys = new SubtitleTimedKey[]
            {
                new SubtitleTimedKey() { key = "so you might as well just quit the game now!", time = 2 },
                new SubtitleTimedKey() { key = "*Glitching*", time = 7 }
            };
            phrases.Add(NullPhrase.Where, where);

            var nothing = Quote("nothing.wav", " Let's see. No items. You're out of stam-");
            nothing.additionalKeys = new SubtitleTimedKey[]
            {
                new SubtitleTimedKey() { key = "*Glitching*", time = 2 },
                new SubtitleTimedKey() { key = "-and uhh I'm faster than you. Uh-oh!", time = 3 },
                new SubtitleTimedKey() { key = "Looks like you're gonna lose and there's liter-", time = 5 },
                new SubtitleTimedKey() { key = "*Glitching*", time = 7 },
                new SubtitleTimedKey() { key = "-nothing you can do.", time = 8 },
            };
            phrases.Add(NullPhrase.Nothing, nothing);

            var stop = Quote("stop.wav", "Please stop. After e-");
            stop.additionalKeys = new SubtitleTimedKey[]
            {
                new SubtitleTimedKey() { key = "*Glitching*", time = 2 },
                new SubtitleTimedKey() { key = "-ing you've experienced in these games,", time = 3 },
                new SubtitleTimedKey() { key = "do you really think the ending i-", time = 6 },
                new SubtitleTimedKey() { key = "*Glitching*", time = 8 },
                new SubtitleTimedKey() { key = "-worth your time?", time = 8.5f},
            };
            phrases.Add(NullPhrase.Stop, stop);

            var scary = Quote("scary.wav", "!emaaaaaaaaaaag eht yortseD !emag eht yortseD");
            scary.additionalKeys = new SubtitleTimedKey[]
            {
                new SubtitleTimedKey() { key = "!emag eht yortseD !emag eht yortseD", time = 3 },
                new SubtitleTimedKey() { key = "*Glitching*", time = 5.7f },
                new SubtitleTimedKey() { key = "...Did that freak you out? Did- Did that scare yo-", time = 6.7f },
                new SubtitleTimedKey() { key = "*Glitching*", time = 10 },
                new SubtitleTimedKey() { key = "Isn't that like, scar- Considered scary?", time = 10f},
            };
            phrases.Add(NullPhrase.Scary, scary);

            poster = AssetLoader.TextureFromMod(NullTeacherPlugin.Instance, "poster.png");
            baldloon = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(NullTeacherPlugin.Instance, "baldloon.png"), 23f);
            nullsprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(NullTeacherPlugin.Instance, "null.png"), 65f);
            lose = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(NullTeacherPlugin.Instance, "NullEnd.wav"), "", SoundType.Effect, Color.white, 0);
            ambient = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(NullTeacherPlugin.Instance, "ambient.mp3"), "", SoundType.Music, Color.black);
            ambient.subtitle = false;
        }

        public static Sprite baldloon;
        public static Sprite nullsprite;
        public static SoundObject lose;
        public static SoundObject ambient;

        public static Texture2D poster;
        public static Dictionary<NullPhrase, SoundObject> phrases = new Dictionary<NullPhrase, SoundObject>();
    }
}
