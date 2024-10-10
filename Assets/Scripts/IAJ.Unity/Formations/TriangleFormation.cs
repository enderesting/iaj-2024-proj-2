using System.Collections;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Game.NPCs;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class TriangleFormation : FormationPattern 
    {
        // This is a very simple line formation, with the anchor being the position of the character at index 0.
        private static readonly float offset = -3.0f;
        public float triangleWidth = 3f; // Width of the triangle (distance between base monsters)
        public float triangleHeight = 4f; // Height of the triangle (distance from tip to base)
        private int orcSpeed = 4;

        public TriangleFormation() : base()
        {
            this.FreeAnchor = true;
        }

        public override Vector3 GetOrientation(FormationManager formation )
        {
            return formation.SlotAssignment.Keys.First().transform.forward;
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber, Vector3 centerMass)
        {
            // Calculate movement direction of anchor
            Vector3 movementDirection = GetOrientation(formation);

            if (movementDirection != Vector3.zero)
            {
                if (slotNumber == 0)
                {
                    return centerMass + offset * orcSpeed * movementDirection;
                }
                else if (slotNumber == 1)
                {
                    return formation.AnchorPosition + GetOrientation(formation) * triangleHeight;
                }
                else if (slotNumber == 2)
                {
                    Vector3 right = Vector3.Cross(GetOrientation(formation), Vector3.up).normalized;
                    return formation.AnchorPosition - right * (triangleWidth / 2);
                }
                else if (slotNumber == 3)
                {
                    Vector3 right = Vector3.Cross(GetOrientation(formation), Vector3.up).normalized;
                    return formation.AnchorPosition + right * (triangleWidth / 2);
                }
            }
            Debug.LogError("TriangleFormation: GetOrientation returned Vector3.zero");
            return Vector3.zero;
        }

        public override  bool SupportSlot(int slotCount)
        {
            return (slotCount <= 3); 
        }
    }
}