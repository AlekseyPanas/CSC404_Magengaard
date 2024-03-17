
using System.Collections.Generic;
using UnityEngine;

/** 
* Defines a set of colors for an element
*/
public class SpellElementColorPalette {
    public enum PaletteStrategy {
        LOOP = 0,
        GET_LAST = 1
    }

    private List<Color> _cols;

    public SpellElementColorPalette(List<Color> colors) {
        _cols = colors;
    }

    /** 
    * Get the n-ary color. index = 0 gets the primary color, 1 gets secondary, 2 tertiary, etc
    * If not enough colors, high indexes start returning 
    */
    public Color GetNaryColor(int index, PaletteStrategy strategy = PaletteStrategy.LOOP) {
        if (index < _cols.Count) { return _cols[index]; }
        else {
            if (strategy == PaletteStrategy.LOOP) {
                return _cols[index % _cols.Count];
            } else if (strategy == PaletteStrategy.GET_LAST) {
                return _cols[_cols.Count - 1];
            } else {
                return new Color(0, 0, 0, 0);  // Impossible code path
            }
        }
    }
}


public class FirePalette : SpellElementColorPalette {
    public FirePalette() : base(new List<Color>(){ 
        new Color(255f/255f, 77f/255f, 0),
        new Color(255f/255f, 149f/255f, 0)
    }) {}
}


public class IcePalette : SpellElementColorPalette {
    public IcePalette() : base(new List<Color>(){ 
        new Color(0, 85f/255f, 255f/255f),
        new Color(0, 229f/255f, 255f/255f)
    }) {}
}

public class WindPalette : SpellElementColorPalette {
    public WindPalette() : base(new List<Color>(){ 
        new Color(9f/255f, 255f/255f, 0),
        new Color(162f/255f, 255f/255f, 0)
    }) {}
}

public class LightningPalette : SpellElementColorPalette {
    public LightningPalette() : base(new List<Color>(){ 
        new Color(140f/255f, 0, 255f/255f),
        new Color(234f/255f, 0, 255f/255f)
    }) {}
}
