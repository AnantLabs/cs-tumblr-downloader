Simple application designed to download images from Tumblr blogs with ease.

There are two download modes, they are:

a - (Download All) All images are downloaded from the Tumblr blogs specified within the settings file. The download stops when there are no images left to download. If images that have already been downloaded are encoutered, they will be skipped.

b - (Update) All of the Tumblr blogs specified within the settings file are scanned for new images. If left running, the program will automatically re-scan the blogs every 30 minutes.

The settings file format is as such:



&lt;settings&gt;


> 

&lt;tumblrAccount&gt;

tumblraccountname1

&lt;/tumblrAccount&gt;


> 

&lt;tumblrAccount&gt;

tumblraccountname2

&lt;/tumblrAccount&gt;


> 

&lt;tumblrAccount&gt;

etc

&lt;/tumblrAccount&gt;




&lt;/settings&gt;



Programming in C#.NET, requires .NET framework 2.0+ which is available for free download on the Microsoft website.

Can be used in Linux by simply running it with MonoDevelop.