using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

// Abstract interface for the chasing entity (the cat).
// Decouples motion logic from the actual character representation,
// allowing different character types (skinned mesh, simple mesh, etc.).
public interface PersecutorBase
{
	Vec3 GetPosition();
	quat GetRotation();
	void SetPosition(Vec3 newPosition);
	// Takes a direction, not a rotation: the implementation smooths the turn itself
	void SetRotation(Vec3 targetDirection);
	void SetAnimation(float persecutorSpeed, bool reachedTarget);
}
