using DiffPlex;

//var a = "12345678901234567890";
//var b = "123x567a90xxx345678xx0";
//var c = "123x567b90yx345678xy0";

// conflict, ABD, pass
//var a = "AB";
//var b = "ABC";
//var c = "ABD";

// 
var a = "ABX";
var b = "ABCCCX";
var c = "ABDX";

// conflict, ab, pass
//var a = "a";
//var b = "ab";
//var c = "ab";

// conflict, pass
//var a = "123456789";
//var b = "1234xx789";
//var c = "123yy6789";

// conflict, pass
//var a = "123456789";
//var b = "12xxxx789";
//var c = "123yy6789";

// conflict, pass
//var a = "11111";
//var b = "11x11";
//var c = "11xy1";

Console.WriteLine(a);

var d0 = Differ.CreateDiffs(a.ToCharArray(), b.ToCharArray());
var p0 = new Patch<char>(d0);
var bb = new string(p0.Apply(a.ToCharArray()).ToArray());
Console.WriteLine($"{bb} {b == bb}");

var d1 = Differ.CreateDiffs(a.ToCharArray(), c.ToCharArray());
var p1 = new Patch<char>(d1);
var cc = new string(p1.Apply(a.ToCharArray()).ToArray());
Console.WriteLine($"{cc} {c == cc}");

var pm = Merger.Merge(p0, p1);
Console.WriteLine(new string(pm.Apply(a.ToCharArray()).ToArray()));

Console.WriteLine();

//Console.WriteLine(Merger.IsOverlap(3, 0, 3, 0));
//Console.WriteLine(Merger.IsOverlap(3, 0, 4, 0));
//Console.WriteLine(Merger.IsOverlap(3, 1, 4, 3));
//Console.WriteLine(Merger.IsOverlap(5, 3, 3, 2));
//Console.WriteLine(Merger.IsOverlap(3, 5, 4, 4));
//Console.WriteLine(Merger.IsOverlap(3, 5, 4, 3));
//Console.WriteLine(Merger.IsOverlap(3, 5, 3, 3));