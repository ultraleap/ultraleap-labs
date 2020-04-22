# Mid-Air Haptic Texture: Modulating Sensation Intensity from Image Displacement Maps
This textures open-source project is part of an internal research project exploring the possibility of rendering varying surface textures using a UH device. This work currently enables the generation of haptic textures from images by modulating haptic sensation intensity via image displacement map greyscale values. From this information, the roughness and bumpiness of a texture can be effectively presented using ultrasonic mid-air haptics. In addition, both visual and haptic feedback are directly linked which ensures congruency for the user whilst exploring a visuo-haptic texture.

## About
This repository contains the following:

* A Unity project that demonstrates the generation of haptic textures.
* C# scripts responsible for processing and rendering different haptic textural sensations.
* Two simple (2D & 3D) demo scenes presenting their application in 2D and 3D environments.
* A small repository of sample textures.
* A customised texture shader, that contains variables related to haptic sensation (smoothness, intensity range).
* A basic texture material with the customised texture shader attached.
* Prefab gameobjects with correct components, material, and shader attached: 
	- 2D Plane gameobject.
	- 3D Cube demo gameobject.

This Unity project can be run on both Ultrahaptics "STRATOS" series hardware (Inspire & Explore).

#### What does this project do?
Contained within this project are scripts that enable the generation of haptic textures. Images and their associated displacement maps can be imported then used to modulate the intensity of a haptic sensation. This modulation produces variation in two textural qualities: roughness and bumpiness. From this information, a variety of material surface topographies can be simulated.

You can use your own textured images provided they have an appropriate displacement map.

Visual feedback and haptic feedback are coupled when using this particular rendering method, which results in excellent congruency between these modalities.

Ultrasonic mid-air haptic textures can be produced within 2D and 3D environments, offering the possiblility to explore many different use cases for numerous applications. 

## Prerequisites
In order explore this project and create your own haptic textures there are some requirements which must first be considered:

#### Hardware

* Ultrahaptics STRATOS platform hardware device (Inspire or Explore)
* Leapmotion Hand Tracking device

#### Software
* Ultrahaptics SDK 2.6.5: https://developer.ultrahaptics.com/downloads/sdk/
* Leap Motion SDK
	- (Windows: v4 https://developer-archive.leapmotion.com/downloads/external/v4-developer-beta/windows)
 	- (Mac: v2 https://developer.leapmotion.com/sdk/v2)
* Leap Motion Core Assets Unity Package: https://developer.leapmotion.com/unity
* This project was built using Unity 2019.2.11f1, but should work with other Unity versions with minimal effort.

## Usage

#### Setting up the project
1. Download the project and copy it into your relevant workspace. 
2. Navigate to the appropriate folder in your Unity Hub / Unity Standalone and open the project.
3. Once open you will see a host of errors, but don't panic. The steps below will address these:
	- Import the Leap Motion Core Assets Unity package, and the Ultrahaptics Unity package into the project (found in your Ultrahaptics SDK folder). When prompted, press "Import".
4. The project should now be ready to use!
	- **TIP:** *It is worth double checking that the LeapHandController Gameobject in the scene contains the HandData.cs script found in: /Assets/Scripts/HandData*

#### Creating a new scene
The following describes how to set up a new scene in a similar fashion to our demo scenes, in order to interact with Haptic Textures.

1. Add the Leap Hand Controller prefab to the scene (Assets/LeapMotion/Prefabs/Misc/LeapHandController) and set it up per your preference.
2. Add the HandData script to the Leap Hand Controller game object.
3. Add the [Haptics] prefab to the scene (Assets/Prefabs/[Haptics])
4. On [Haptics], set the amount of hands you require in the scene, 2 for Left and Right. Set the Chirality of each of these accordingly.Then assign the "ScanPosition" (we currently use bone2 of the middle finger on the left hand and right hand).
5. Setup the HapticRunner Hand Positions in the in the same manner.
     - **TIP:** *Scan position in Haptic Renderer is the position on the hand where we raycast from onto the texture, in order to find the displacement. Hand Positions in Haptic Runner are where we then project the haptic feedback. Feel free to change and experiment with these values!*
6. Add the model you want to apply textures into the scene. 
7. The model needs the following:
     - A mesh collider (non-convex).
     - A Texture Attributes script.

#### Texture Importing
The scene contains a number of different textures that are located in: /Assets/Textures/. These are ready to be used and can be applied to any game object. 

However, it is possible to create your own textures!

To do so follow these steps:

1. Create a new folder within the /Assets/Textures/ folder entitled the texture/image you wish to use.
2. Grab the "BasicTextureMaterial" in /Assets/Prefabs/ and copy it into your newly created folder. Rename it.
3. In this folder, copy in your texture image, and displacement map (grey-scale image), along with any additional material component you wish to use (metallic, normal map etc).
	- **TIP:** *Make sure to enhance the contrast and saturation on your displacement map, so separation between white and black values is clearly distinguished. This will improve haptic rendering.*
4. Select the material in Unity and in each of its corresponding texture selection options, assign the correct image. i.e. place the displacement map into the "Disp Texture" selection window.
5. Select the displacement map from the texture folder. Ensure that "Read/Write Enable" is se to "True" from within the "Advanced" drop-down menu.

You will see that on your newly created texture, a "BasicTextureShader" will be assigned. This shader contains a number of haptic parameters that can be altered in order to adjust the sensation.

* **Haptic Smoothness** - This value can be adjusted from 20 - 80. A higher value will yield a smoother sensation.
* **Haptic Intensity Minimum/Maximum** - These values will allow you to adjust the variance between the intensity values used for the haptic sensation. It is best to keep the maximum setting at 1. Bringing the minimum value closer to 1 will result in a flatter texture, and again feel smooth.

## License
This project is open-sourced under the Apache V2 license. The textures used within this repo were obtained at https://www.cc0textures.com.

## Support, Contact & Contribution
For any questions regarding this project, please contact david.beattie@ultraleap.com. Alternatively, please branch this repo and leave comments so we can keep up to date with any requests and issues.

## Acknowledgements
This project has received funding from the European Union’s Horizon 2020 research and innovation programme under grant agreement No 801413, H-Reality.