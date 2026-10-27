using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private Vector2 cameraBack, cameraTarget, cameraVelocity, shakeOffset;
        private float cameraElapsed;
        private int cameraRelativeTicks, shakeTicks, shakeType;
        private bool cameraMoving, cameraWait, introductory;
        private int cameraSpeed;
        public bool CameraMoving => cameraMoving;
        public Vector2 CameraPosition => cameraPoint;
        public Vector2 CameraShakeOffset => shakeOffset;
        public bool Introductory => introductory;

        private Vector2 ClampCamera(Vector2 center)
        {
            if (Map == null) return center;
            return new Vector2(Mathf.Clamp(center.x, ViewWidth / 2, Mathf.Max(ViewWidth / 2, Map.Width - ViewWidth / 2)),
                Mathf.Clamp(center.y, ViewHeight / 2, Mathf.Max(ViewHeight / 2, Map.Height - ViewHeight / 2)));
        }
        private void MoveCameraRelative(int x, int y, int step, bool save)
        {
            if (save) cameraBack = cameraPoint;
            cameraSpeed = Mathf.Max(1, Mathf.Abs(step));
            cameraVelocity = new Vector2(x == 0 ? 0 : Mathf.Sign(x) * cameraSpeed, y == 0 ? 0 : Mathf.Sign(y) * cameraSpeed);
            cameraRelativeTicks = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) / cameraSpeed;
            cameraMoving = cameraRelativeTicks > 0; cameraWait = false; cameraElapsed = 0; fixedCamera = true;
        }
        private void MoveCameraTo(Vector2 destination, int step, bool save)
        {
            if (save) cameraBack = cameraPoint;
            cameraTarget = ClampCamera(destination); cameraSpeed = Mathf.Max(1, Mathf.Abs(step));
            cameraRelativeTicks = 0; cameraMoving = cameraWait = true; cameraElapsed = 0; fixedCamera = true;
        }
        private void SetCameraPosition(Vector2 position)
        {
            cameraBack = cameraPoint; cameraPoint = ClampCamera(position); cameraMoving = cameraWait = false; fixedCamera = true;
        }
        private void SetShake(int ticks, int type)
        { shakeTicks = ticks == -1 ? int.MaxValue : Mathf.Max(0, ticks); shakeType = type; if (shakeTicks == 0) shakeOffset = Vector2.zero; }
        private void ResetCameraEffects()
        { cameraMoving = cameraWait = false; cameraRelativeTicks = shakeTicks = 0; cameraElapsed = 0; shakeOffset = Vector2.zero; }
        private void TickCameraEffects(float delta)
        {
            if (menu || reinforceOpen || AtTitle) return;
            cameraElapsed += delta;
            while (cameraElapsed >= .08f)
            {
                cameraElapsed -= .08f;
                if (cameraMoving)
                {
                    if (cameraRelativeTicks > 0)
                    {
                        cameraPoint = ClampCamera(cameraPoint + cameraVelocity);
                        if (--cameraRelativeTicks == 0) cameraMoving = false;
                    }
                    else
                    {
                        cameraPoint = new Vector2(Mathf.MoveTowards(cameraPoint.x, cameraTarget.x, cameraSpeed), Mathf.MoveTowards(cameraPoint.y, cameraTarget.y, cameraSpeed));
                        if (cameraPoint == cameraTarget) cameraMoving = cameraWait = false;
                    }
                }
                if (shakeTicks > 0)
                {
                    int phase = shakeTicks % 4;
                    shakeOffset = shakeType == 0 ? (phase == 1 ? new Vector2(4, 0) : phase == 3 ? Vector2.zero : new Vector2(2, 2))
                        : new Vector2(0, phase == 0 ? 2 : phase == 1 ? -2 : phase == 2 ? 1 : -1);
                    if (--shakeTicks == 0) shakeOffset = Vector2.zero;
                }
            }
        }
    }
}
