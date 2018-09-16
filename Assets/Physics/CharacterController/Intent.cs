
public class Intent
{
    public enum Type { JUMP, JETPACK,
        H_MOVE_LEFT, H_MOVE_RIGHT,
        ROLL, ATTACK, H_MOVE_NONE
    };

    public readonly Type type;

    public Intent(Type type)
    {
        this.type = type;
    }
}
