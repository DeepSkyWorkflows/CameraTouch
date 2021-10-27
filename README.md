# Camera Touch

Camera touch is a command line tool for organizing the raw files that come from your camera. You can view properties, gather statistics, move and/or copy.

> ⚠**WARNING** Always run with the `-s` or `--scan-only` option first to verify your options before modifying files!

## Usage

The following are examples of ways to use the utility:

**Grab statistics only**
`cameratouch --info-statistics --recurse-subdirectories --scan-only "C:\myfiles"`

**Show stats per file**
`cameratouch --info-statistics --properties-display --recurse-subdirectories --scan-only "C:\myfiles"`

**Show stats for a single file**
`cameratouch --properties-display --scan-only "C:\myfiles\myfile.arw"`

**Rename in place with defaults**
`cameratouch --move-file "C:\myfiles"`

**Copy to a new directory and organize by date then exposure time and ISO and name the file with exposure time, iso, and focal length**
`cameratouch --directory-specs $dt[YYYY-MM-DD];$et_$is --file-naming-spec $et_$is_$fl --recurse-subdirectories "C:\myfiles" "D:\targetFiles"`

The following options are available:

|Short Code|Long Code|Default|Description|
|---|---|---|---|
|-s|--scan-only|`false`|Scan only. Do not move or rename files.|
|-r|--recurse-subdirectories|`false`|Recurse subdirectories.|
|-p|--properties-display|`false`|Show properties for each file.|
|-m|--move-file|`false`|Move the file instead of copying.|
|-i|--info-statistics|`false`|Show aggregaed statistics about properties.|
|-d|--directory-specs|`null`|Directory naming specifications. Use the pattern template and separate directories with a semi-colon.|
|-f|--file-naming-spec|`$et_$is_$fl_Image`|File naming spec. Use the pattern template.|
|  |--help| |Display this help screen.|
|  |--version| |Display version information.|
| |Source (pos. 0)| |Required. The path to the file or directory to scan.|
| |TargetDirectory (pos. 1)| |Optional. The root of the target directory to move files to. Defaults to source.|


The following codes are valid for the file and directory specs. Sequence numbers are automatically added for file specs. Other characters "pass through".

|Template code|Description|Example Value|
|---|---|---|
|$cf|CFA Pattern|RGGB|
|$cp|Compression|Sony ARW Compressed|
|$dt[format]|Date/Time (format follows the C# data format specification) ex: `$dt[yyyy-MM-dd]`|2021-10-26|
|$et|Exposure Time|0.0025s|
|$ex|Expected File Name Extension|ARW|
|$fd|Detected File Type Long Name|Sony Camera Raw|
|$fl|Focal Length|206mm|
|$fn|File Name|DSC12345|
|$fs|F-Number|6.3|
|$ft|Detected File Type Name|ARW|
|$ht|Image Height|1080|
|$is|ISO Speed Ratings|3200|
|$it|Photometric Interpretation|Color Filter Array|
|$lm|Lens Model|E 55-210mm F.45-6.3 OSS|
|$ls|Lens Specification|55-210mm f/4.5-6.3|
|$md|Model|ILCE-6300|
|$mk|Make|Sony|
|$or|Orientation|Left side, bottom (Rotate 270 CW)|
|$ru|Resolution Unit|Inch|
|$sf|Software|ILCE-6300 v2.01|
|$sz|File Size|234234234323|
|$wd|Image Width|1920|
|$xr|X Resolution|300|
|$yr|Y Resolution|300|


