using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArkWalk
{
    public sealed class WalkSite : MonoBehaviour
    {
        public string Id { get; private set; }
        public Vector3 Spawn;
        public float Yaw;
        public float Pitch;
        public Vector3 Exit = new Vector3(0, 0, 12);
        public readonly List<WalkMark> Marks = new List<WalkMark>();
        private WalkApp app;
        private WalkMark paper;
        private float paperTime;

        internal void Open(WalkApp owner, string id)
        {
            app = owner;
            Id = id;
            if (!ArkVoyage.IsOrbit(id)) throw new InvalidOperationException("Unknown orbital destination");
            DistrictWorld.Build(this);
            // No interior registry attached.
        }

        public GameObject Mark(Vector3 worldPos, float radius, string title, string speech, Action action = null)
        {
            var anchor = new GameObject(title);
            anchor.transform.SetParent(transform, false);
            anchor.transform.position = worldPos;
            var mark = anchor.AddComponent<WalkMark>();
            mark.Title = title;
            mark.Speech = speech;
            mark.Radius = radius;
            mark.OnTap = action;
            Marks.Add(mark);
            return anchor;
        }

        public void OfferStar(Action offer, Action accept, Transform star)
        {
            if (!star) return;
            if (Vector3.Distance(app.Eye.transform.position, star.position) > 2.65f)
            {
                app.Tell("阿禾", "再走近一点，就能看见纸星上的折痕。");
                return;
            }
            if (paper && paper.gameObject.activeInHierarchy)
            {
                if (Time.time > paperTime + 0.85f) TakeStar(accept);
                return;
            }
            offer?.Invoke();
            app.Pilot.LookAtPaper();
            paperTime = Time.time;
            if (!paper)
            {
                paper = Mark(star.position, 0.19f, "接纸星", "", () => TakeStar(accept)).GetComponent<WalkMark>();
                paper.Follow = star;
                paper.Paper = true;
            }
            paper.gameObject.SetActive(true);
        }

        private void TakeStar(Action accept)
        {
            if (!paper || !paper.isActiveAndEnabled || Time.time < paperTime + 0.65f) return;
            paper.gameObject.SetActive(false);
            accept?.Invoke();
            app.Pilot.StandUp();
            app.Tell("纸星", "这颗小星星，送给今天来舰上做客的你。");
        }

        public void FlightMap() { app.Hud.OpenRoutes(true); }
        public void ReturnToDock() { app.ReturnToDock(); }
        public void Visit(string id) { app.Go(id); }
        public void Tell(string heading,string speech) { app.Tell(heading,speech); }
    }

    public sealed class WalkMark : MonoBehaviour
    {
        public string Title;
        public string Speech;
        public float Radius;
        public Action OnTap;
        public Transform Follow;
        public bool Portal;
        public bool Paper;
        public Vector3 Point => Follow ? Follow.position : transform.position;
    }
}
