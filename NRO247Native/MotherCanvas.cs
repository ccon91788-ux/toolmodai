public class MotherCanvas
{
	public static MotherCanvas instance;

	public GameCanvas tCanvas;

	public int zoomLevel = 1;

	public Image imgCache;

	private int[] imgRGBCache;

	private int newWidth;

	private int newHeight;

	private int[] output;

	private int OUTPUTSIZE = 20;

	public MotherCanvas()
	{
		checkZoomLevel(getWidth(), getHeight());
	}

	public void checkZoomLevel(int w, int h)
	{
	}

	public int getWidth()
	{
		return (int)ScaleGUI.WIDTH;
	}

	public int getHeight()
	{
		return (int)ScaleGUI.HEIGHT;
	}

	public void setChildCanvas(GameCanvas tCanvas)
	{
		this.tCanvas = tCanvas;
	}

	protected void paint(mGraphics g)
	{
		tCanvas.paint(g);
	}

	protected void keyPressed(int keyCode)
	{
		tCanvas.keyPressedz(keyCode);
	}

	protected void keyReleased(int keyCode)
	{
		tCanvas.keyReleasedz(keyCode);
	}

	protected void pointerDragged(int x, int y)
	{
		tCanvas.pointerDragged(x, y);
	}

	protected void pointerPressed(int x, int y)
	{
		tCanvas.pointerPressed(x, y);
	}

	protected void pointerReleased(int x, int y)
	{
		tCanvas.pointerReleased(x, y);
	}

	public int getWidthz()
	{
		return getWidth();
	}

	public int getHeightz()
	{
		return getHeight();
	}
}
