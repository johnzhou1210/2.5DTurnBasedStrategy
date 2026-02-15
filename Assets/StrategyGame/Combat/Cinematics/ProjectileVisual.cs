using System;
using DG.Tweening;
using UnityEngine;

namespace StrategyGame.Combat.Cinematics {
    public class ProjectileVisual : MonoBehaviour {
        [SerializeField] private Transform impactPointTransform;
        public void Setup(CombatPuppet puppetSource, ProjectileVisualData data, bool hit, bool isBreak) {
            Vector3 startPos = transform.position;
            Vector3 targetPos = puppetSource.CombatDirector.IsAttackerTurn ? puppetSource.CombatDirector.defenderPuppet.transform.position : puppetSource.CombatDirector.attackerPuppet.transform.position;
            float travelTime = data.TravelTime;
            Vector3 direction = (targetPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetPos);
            Vector3 lastPosition = startPos;

            bool targetReacted = false;

            // 1. Determine the 'End Value' for the tween
            // If it hits, we stop at 1. If it misses, we go further (e.g., to 2.0)
            float endValue = hit ? 1f : 2f;
            float totalDuration = hit ? travelTime : travelTime * 2f;
            DOVirtual.Float(0, endValue, totalDuration, (float value) => {
                // 2. Interpolate position beyond the target
                Vector3 currentPos = Vector3.LerpUnclamped(startPos, targetPos, value);

                // 3. Evaluate the curve beyond the 0-1 range
                // Note: Set your AnimationCurve to 'Post-Extrapolation: Linear' or 'PingPong' 
                // in the Unity Inspector for predictable behavior past value 1.0.
                float heightOffset = data.HeightCurve.Evaluate(value) * data.MaxArchHeight;
                transform.position = new Vector3(currentPos.x, currentPos.y + heightOffset, currentPos.z);

                // Rotation Logic (same as before)
                Vector3 diff = transform.position - lastPosition;
                if (diff.sqrMagnitude > 0.0001f) {
                    float horizontalDist = new Vector2(diff.x, diff.z).magnitude;
                    float angle = Mathf.Atan2(diff.y, horizontalDist) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.LookRotation(direction);
                    transform.Rotate(0, 0, angle);
                }
                transform.localEulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
                lastPosition = transform.position;

                // 4. Trigger impact exactly when we cross the targetPos (value = 1)
                if (!targetReacted && value >= 1f) { // Small threshold check
                    targetReacted = true;
                    puppetSource.SpawnImpactVFX(data, impactPointTransform.position, isBreak, hit);
                }
            }).SetEase(Ease.Linear).OnComplete(() => {
                Destroy(gameObject);
            });
        }
    }
}
