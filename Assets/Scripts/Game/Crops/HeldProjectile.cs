using UnityEngine;

public abstract class HeldProjectile : MonoBehaviour
{
    protected Transform holder;
    protected bool isHeld;

    public virtual void OnPickedUp(Transform holdPoint, GameObject player)
    {
        holder = holdPoint;
        isHeld = true;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        DisableCollisions(player);

        Debug.Log($"{name}: Picked up and held.");
    }

    protected virtual void DisableCollisions(GameObject player)
    {
        Collider2D myCol = GetComponent<Collider2D>();
        Collider2D playerCol = player.GetComponent<Collider2D>();

        if (myCol && playerCol)
        {
            Physics2D.IgnoreCollision(myCol, playerCol);
        }
    }
    // Future
    public abstract void Throw(Vector2 direction, float force);
}