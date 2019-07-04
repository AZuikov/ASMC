Example of using Barcoder:

SaveFileDialog saveFileDialog = new SaveFileDialog();
saveFileDialog.ShowDialog();

var renderer = new ImageRenderer();
renderer.SetMargin(10,0);
renderer.DrawString = true;

var code1 = Code128Encoder.Encode(TextBox.Text);
using (var stream = new FileStream(saveFileDialog.FileName + "Code128.bmp", FileMode.Create))
{
	renderer.Render(code1, stream);
}

var code2 = DataMatrixEncoder.Encode(TextBox.Text);
using (var stream = new FileStream(saveFileDialog.FileName + "DataMatrix.bmp", FileMode.Create))
{
	renderer.Render(code2, stream);
}