using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

public class customJoystick : MonoBehaviour

{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [DisplayStringFormat("Custom Joystick {xAxis},{yAxis}")]
    public class CustomJoystickComposite : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Button")] public int xAxis;
        [InputControl(layout = "Button")] public int yAxis;

        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            var x = context.ReadValue<float>(xAxis);
            /*
             * its reversed for some damn reason (Tested with Logitech F710)
             */
            var y = context.ReadValue<float>(yAxis) * -1;
            return new Vector2(x, y);
        }

        static CustomJoystickComposite()
        {
            InputSystem.RegisterBindingComposite<CustomJoystickComposite>();
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
        }
    }
}