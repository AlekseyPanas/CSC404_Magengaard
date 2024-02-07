/**
 * Implement this to listen for effects of type T.
 */
public interface IEffectListener<T>
{
    void OnEffect(T effect);
}
