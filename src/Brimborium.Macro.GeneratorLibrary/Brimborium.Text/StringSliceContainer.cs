namespace Brimborium.Text;

public sealed class StringSliceContainer {
    public StringSliceContainer() {
        this.Value = new StringSlice();
    }

    public StringSliceContainer(StringSlice value) {
        this.Value = value;
    }

    public StringSlice Value;
}
