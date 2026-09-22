# C# Code Samples


## Animation Characters

- [Additive Animation Blending [Animation Graph]](animation_characters/additive_animation_blending/README.md): Additive blending of two animations.
- [Bones: Partial Blend [Animation Graph]](animation_characters/bones_partial_blend/README.md): Demonstration of partial blending between two animations using bone-specific interpolation.
- [Linear Animation Blending [Animation Graph]](animation_characters/linear_animation_blending/README.md): Linear interpolation of two animations.

## Animation Generic

- [Animation Layers Playback](animation_generic/animation_layers_playback/README.md): Demonstration of animation sequences played in order or simultaneously.
- [Bones: Rotation [Animation Graph]](animation_generic/bones_rotation/README.md): Blending skeletal animations in an animation graph and rotating a bone programmatically.
- [Curve Animation](animation_generic/curve_animation/README.md): Real-time animation of transformations and material parameters using Curve2D for flexible, non-linear motion.
- [Global Engine Parameters Animation](animation_generic/global_engine_parameters_animation/README.md): Animating global Engine parameters, such as physics gravity and render background color, using singleton channels.
- [Material Parameters Animation](animation_generic/material_parameters_animation/README.md): Changing material parameters at runtime.
- [Node Parameters Animation](animation_generic/node_parameters_animation/README.md): Animating position, rotation, and scale of a node using node binds and different types of animation channels.
- [Physics-Based Animation](animation_generic/physics_based_animation/README.md): Physics-based animation of movements using easing functions and spring simulation.
- [Property Animation](animation_generic/property_animation/README.md): Animating a parameter of a property assigned to a node and using the animated value to drive the logic of the sample.
- [Track Playback](animation_generic/track_playback/README.md): Using Tracker to animate the position, rotation, and scale of an object.
- [Widget Animation](animation_generic/widget_animation/README.md): Animating widget position, font size, and color using runtime binds.

## App Logic

- [Advanced Event Connection Patterns](app_logic/advanced_event_connection_patterns/README.md): Advanced ways of subscribing to events in UNIGINE: using extra arguments, discarding parameters, and storing connection handles for disconnection.
- [Arcade Game Prototype](app_logic/arcade_game_prototype/README.md): A simple yet flexible 3D shooter prototype featuring core gameplay systems like shooting, collisions, health management, and dynamic effects.
- [Component Parameters In Editor](app_logic/component_parameters_in_editor/README.md): Demonstration of component parameter types and configuration options.
- [Console Interaction](app_logic/console_interaction/README.md): Interacting with the Engine's built-in console and adding custom console commands and variables via API using the Console and ConsoleVariable classes.
- [Euler Angle Composition And Decomposition](app_logic/euler_angle_composition_and_decomposition/README.md): Composing and decomposing object rotation using Euler angles in different axis sequences.
- [Filesystem External Package](app_logic/filesystem_external_package/README.md): Demonstration of working with external package files via the Package class.
- [Inverse FPS Usage](app_logic/inverse_fps_usage/README.md): Using Game.IFps to implement movement logic independent of the frame rate.
- [XML](app_logic/xml/README.md): Creating and manipulating an XML document using the Xml class.

## Csharp Language Features

- [Abstract Components](csharp_language_features/abstract_components/README.md): Demonstrating the use of abstract component classes for shared behavior via C# API.
- [Coroutine Animations](csharp_language_features/coroutine_animations/README.md): Managing (starting, stopping, and coordinating) multiple coroutines in UNIGINE to create non-blocking, time-based animations with UI-driven runtime control.

## Input Handling

- [Input Gamepad](input_handling/input_gamepad/README.md): This sample demonstrates how to add input from the gamepad to the project.
- [Input Joystick](input_handling/input_joystick/README.md): This sample demonstrates how to add advanced joystick input handling, supporting multiple controllers with real-time axis/button monitoring and force feedback effects in UNIGINE.
- [Input Keyboard And Mouse](input_handling/input_keyboard_mouse/README.md): This sample demonstrates how to add monitoring of keyboard and mouse input, tracking key states, mouse movements, wheel events, cursor positions, and real-time input data.
- [Touch](input_handling/touch/README.md): This sample demonstrates how to add multi-touch input from the touchscreen, visualizing finger positions with dynamic circles and displaying real-time coordinates to the project using the InputTouches.cs component assigned to NodeDummy.

## Multi Threading Performance Optimization

- [Asynchronous Meshes And Textures Loading](multi_threading_performance_optimization/asynchronous_meshes_and_textures_loading/README.md): Loading meshes and textures in a separate thread using the AsyncQueue class.
- [Asynchronous Nodes Loading Stress-Test](multi_threading_performance_optimization/asynchronous_nodes_loading_stress_test/README.md): Asynchronous node loading via AsyncQueue with main-thread spatial integration.
- [Asynchronous Tasks Scheduler Configuration](multi_threading_performance_optimization/asynchronous_tasks_scheduler_configuration/README.md): Managing tasks via AsyncQueue class with dirrefent thread types, parallel execution and frame control.
- [Microprofiler Custom Counters](multi_threading_performance_optimization/microprofiler_custom_counters/README.md): Using Microprofile, an advanced CPU/GPU profiler, to track performance and estimate the time spent on different sections of code.

## Navigation

- [Experimental Navigation Mesh](navigation/experimental_navigation_mesh/README.md): Configuring pathfinding between two points around obstacles using Experimental Navigation Mesh.
- [Experimental Navigation Mesh Character](navigation/experimental_navigation_mesh_character/README.md): Moving an animated character over an Experimental Navigation Mesh with the root motion of the animation graph.
- [Experimental Navigation Mesh Conversion](navigation/experimental_navigation_mesh_conversion/README.md): Converting navigation data between the classic Navigation Mesh and the Experimental Navigation Mesh.
- [Experimental Navigation Mesh Crowd](navigation/experimental_navigation_mesh_crowd/README.md): Moving a crowd of agents over an Experimental Navigation Mesh with collision avoidance.
- [Experimental Navigation Mesh Demo](navigation/experimental_navigation_mesh_demo/README.md): Configuring pathfinding to multiple targets on a plane with obstacles using Experimental Navigation Mesh.
- [Experimental Navigation Mesh Queries](navigation/experimental_navigation_mesh_queries/README.md): Performing spatial queries on an Experimental Navigation Mesh: nearest and reachable points, random points, boundaries, and polygons in an area.
- [Experimental Navigation Mesh Terrain](navigation/experimental_navigation_mesh_terrain/README.md): Navigating a large terrain using two levels of detail: a coarse Experimental Navigation Mesh for the whole world and a detailed one streamed and baked around the agents at runtime.
- [Navigation Mesh 2D](navigation/navigation_mesh_2d/README.md): Calculating and visualizing 2D navigation paths using a Navigation Mesh object and PathRoute class.
- [Navigation Mesh 2D Demo](navigation/navigation_mesh_2d_demo/README.md): Calculating and visualizing 2D navigation paths with moving targets using Navigation Mesh object and PathRoute class.
- [Navigation Obstacles 2D](navigation/navigation_obstacles_2d/README.md): Demonstrating the use of Obstacles within a Navigation Mesh to dynamically modify valid pathfinding areas at runtime.
- [Navigation Sectors 2D](navigation/navigation_sectors_2d/README.md): Calculating and visualizing 2D navigation paths using Navigation Sector objects and the PathRoute class.
- [Navigation Sectors 3D](navigation/navigation_sectors_3d/README.md): Calculating and visualizing 3D navigation paths using Navigation Sector objects and the PathRoute class.
- [Navigation Sectors 3D Demo](navigation/navigation_sectors_3d_demo/README.md): Calculating and visualizing 3D navigation paths using Navigation Sector and the PathRoute class to track dynamic targets.

## Network

- [HTTP Image Request](network/http_image_request/README.md): This sample shows how to implement an asynchronous HTTP request to a REST API to download image files and apply them to scene objects at runtime.
- [HTTP Request Handling](network/http_request_handling/README.md): Implementing asynchronous HTTP GET requests to external REST API and displaying the retrieved data in the user interface.
- [TCP Sockets](network/tcp_sockets/README.md): Establishing and managing TCP socket connections between a server and multiple clients each represented by a UNIGINE-application. Clients can connect to the server, exchange text messages via the Console, and receive camera transform updates from the server.
- [UDP Sockets](network/udp_sockets/README.md): Using the sockets API to send and receive UDP messages in the network between two peers each represented by a UNIGINE-application.

## Nodes

- [Cluster](nodes/cluster/README.md): Dynamic manipulation of ObjectMeshCluster in UNIGINE, showcasing how to add/remove mesh instances at runtime through user interaction.
- [Water Surface Parameters Fetch](nodes/water_surface_parameters_fetch/README.md): Demonstration of how various parameters influence the accuracy of fetch and intersection operations on the Global Water object across different Beaufort levels.

## Physics

- [Body Events](physics/body_events/README.md): Demonstrating the usage of Frozen, Position, and ContactEnter events of the Body class via C# API.
- [Body Fracture Explosion](physics/body_fracture_explosion/README.md): Simulating a radial explosion that triggers BodyFracture object to crack and applies forces to its pieces.
- [Body Fracture Falling Spheres](physics/body_fracture_falling_spheres/README.md): Continuously falling objects fracturing upon collision implemented using the BodyFracture class.
- [Body Fracture Shooting Gallery](physics/body_fracture_shooting_gallery/README.md): Implementation of a basic physics-driven shooting gallery using Fracture Body.
- [Joint Events](physics/joint_events/README.md): Demonstrating the usage of the Broken event of the Joint class via C# API.
- [Physics Movement](physics/physics_movement/README.md): Simple logic of moving an object using physical methods (by force or by impulse).
- [Update Physics](physics/update_physics/README.md): Demonstration of the difference between implementation of physics-driven movement within the update() and updatePhysics() methods.

## Player Controllers

- [CAD-Style Camera Panning](player_controllers/cad_style_camera_panning/README.md): A camera moving parallel to the screen plane.
- [Camera First Person](player_controllers/camera_first_person/README.md): A first-person spectator-style camera with free movement.
- [Camera Zoom](player_controllers/camera_zoom/README.md): Creating interactive camera system with adjustable zoom and focus on selectable scene targets.
- [First-Person Controller](player_controllers/first_person_controller/README.md): Implementation of a first-person character controller with an advanced movement system and collision detection.
- [Observer Controller](player_controllers/observer_controller/README.md): Implementation of a free-flying camera similar to the one used in the UnigineEditor (with zooming, panning, focusing and speed control).
- [Orbit Camera Controller](player_controllers/orbit_camera_controller/README.md): Creating an orbital camera rotating around a target.
- [Persecutor Controller](player_controllers/persecutor_controller/README.md): A third-person camera following a moving target.
- [Spectator Controller](player_controllers/spectator_controller/README.md): Implementation of a customizable first-person spectator camera with configurable movement and physical collision detection.
- [Top-Down Controller](player_controllers/top_down_controller/README.md): Implementation of some elements of a top-down strategy such as selecting one or multiple units, panning the view, turning the camera and smoothly focusing it on the current selection.

## Procedural Generation Placement

- [Clutter To Cluster Converter](procedural_generation_placement/clutter_to_cluster_converter/README.md): Dynamic generation of a Mesh Clutter and its conversion into a performance-optimized Mesh Cluster at runtime.
- [Grid-Based Node Spawning](procedural_generation_placement/grid_based_node_spawning/README.md): Spawning nodes across a grid.
- [Procedural Mesh Generation](procedural_generation_placement/procedural_mesh_generation/README.md): Demonstrating runtime procedural mesh generation, along with visualization of how different procedural modes impact memory usage during creation and rendering.
- [Procedural Mesh Modification](procedural_generation_placement/procedural_mesh_modification/README.md): Demonstrating runtime procedural mesh modification, along with visualization of how different procedural modes impact streamiing and memory usage during creation and rendering.
- [Procedural Mesh Updates Mesh Clusters](procedural_generation_placement/procedural_mesh_updates_mesh_clusters/README.md): A minimal example that shows how to generate and apply a procedural mesh at runtime using the correct update sequence.
- [Procedural Spline Mesh Generation](procedural_generation_placement/procedural_spline_mesh_generation/README.md): Procedural generation of spline-based meshes from world points with real-time parameter control.
- [Real-Time Mesh Editing with Marching Cubes](procedural_generation_placement/real_time_mesh_editing_marching_cubes/README.md): Simulation of the ground digging process based on mesh geometry modification.
- [Sensor Zone Visualizer](procedural_generation_placement/sensor_zone_visualizer/README.md): Procedural frustum mesh generator for visualizing sensor fields of view with live controls for angles, distance, and color.
- [Timer-Based Node Spawning](procedural_generation_placement/timer_based_node_spawning/README.md): Implementing a simple node spawner that creates nodes at a set spawn frequency.

## Rendering

- [Camera To Texture](rendering/camera_to_texture/README.md): Capturing a camera's output in real time and projecting it onto a material's albedo texture using Viewport.RenderTexture2D().
- [FFP Triangle Fan Rendering](rendering/ffp_triangle_fan_rendering/README.md): Using FFP functionality to draw simple 2D shapes over the rendered image without any additional shaders.
- [Gui To Texture](rendering/gui_to_texture/README.md): Rendering GUI elements into a texture using Gui.Render() and applying the resulting texture to materials.
- [Mesh To Mask Texture](rendering/mesh_to_mask_texture/README.md): Rendering a mesh into a texture using a custom material for mask generation.
- [Procedural 3D Volume Texture Generation](rendering/procedural_3d_volume_texture_generation/README.md): Procedurally generate 3D image data and use it as a density texture for a volume-based material in real time.
- [Screenshot](rendering/screenshot/README.md): Simple demonstration of how to make a screenshot by grabbing the final image from the rendering sequence.
- [Visualizer](rendering/visualizer/README.md): Demonstration of the full range of features provided by the Visualizer class for visual debugging.
- [Weapon Clipping](rendering/weapon_clipping/README.md): Rendering the weapon from a second camera into a texture to avoid clipping issues.

## Scene Management

- [Bounding Box Object Detection](scene_management/bounding_box_object_detection/README.md): Finding an intersection between the bound box and geometry.
- [Bounding Frustum Object Detection](scene_management/bounding_frustum_object_detection/README.md): Finding an intersection between the bound frustum and geometry.
- [Bounding Sphere Object Detection](scene_management/bounding_sphere_object_detection/README.md): Finding an intersection between the bound sphere and geometry.
- [Create Mesh Primitives](scene_management/create_mesh_primitives/README.md): Creating parametric 3D primitives at runtime.
- [Day-Night Cycle](scene_management/day_night_cycle/README.md): Implementation of an automated day-night system with light and material switching.
- [Local vs World Space Transforms](scene_management/local_vs_world_space_transforms/README.md): Demonstration of the difference between local and world transformations.
- [Move By Trajectory](scene_management/move_by_trajectory/README.md): Three types of movement along a predefined path: linear interpolation, spline interpolation, and a trajectory loaded from a *.path file.
- [Node Movement](scene_management/node_movement/README.md): Implementing object movement and rotation in 3D space using three different methods.
- [Node Spawning And Deletion](scene_management/node_spawning_and_deletion/README.md): Dynamic creation and deletion of node instances at runtime.
- [Nodes And Widgets Lifetime Control](scene_management/nodes_and_widgets_lifetime_control/README.md): Implementation of configurable node and widget lifetimes with persistence across different worlds.
- [Raycast Detection And Bitmasking](scene_management/raycast_detection_and_bitmasking/README.md): Finding an intersection of a ray with geometry.
- [Raycast From Mouse Position](scene_management/raycast_from_mouse_position/README.md): Finding an intersection between a ray cast from the camera through the mouse cursor and geometry.
- [Trigger System Examples](scene_management/trigger_system_examples/README.md): Implementation of various types of triggers.

## Sounds

- [FMOD Core](sounds/fmod_core/README.md): FMOD Core integration for real-time playback of 2D and 3D sounds with timeline control and DSP effects. NOTICE: Additional libraries are required, run the sample for details.
- [FMOD Studio](sounds/fmod_studio/README.md): FMOD Studio integration with real-time interactive audio effects, spatialization, and Doppler effect simulation. NOTICE: Additional libraries are required, run the sample for details.
- [Sound Ambient](sounds/sound_ambient/README.md): Playing ambient sounds via code.
- [Sound Reverb](sounds/sound_reverb/README.md): Setting a reverberation zone and its parameters via code.
- [Sound Source 3D](sounds/sound_source_3d/README.md): Playing a sound source via code.

## Terrain Modification Usage

- [Polygon-Based Tools for Procedural Modifications](terrain_modification_usage/polygon_based_procedural_modifications/README.md): Procedural polygon objects generation based on world points for landscape editing and object placement.

## Unigine Script Interop

- [Usc Arrays](unigine_script_interop/usc_arrays/README.md): Integration between C# and UnigineScript by registering external C# functions that manipulate UnigineScript array types.
- [Usc Callbacks](unigine_script_interop/usc_callbacks/README.md): Calling UnigineScript functions from C# code via callbacks.
- [Usc Variables](unigine_script_interop/usc_variables/README.md): Working with different variable types in UnigineScript using the Variable class from C# code.

## User Interface

- [Fonts](user_interface/fonts/README.md): Multi-script text rendering (English, Russian, Arabic, Thai, Chinese, Hindi) across GUI widgets, 3D ObjectText, and world-space ObjectGui, with configurable text direction, cursor mode, and font size.
- [Measurements](user_interface/measurements/README.md): Dynamically measuring and visualizing the distance from a movable node to surrounding target nodes.
- [Object Frame](user_interface/object_frame/README.md): Using the WidgetCanvas class to render custom frames around objects in the viewport and save frame metadata as JSON.
- [Object Text](user_interface/object_text/README.md): Demonstration of 3D text objects with adjustable parameters, animated colors, and rich formatting using the ObjectText class.
- [Target Marker](user_interface/target_marker/README.md): Implementation of a marker that always highlights the target when it is within the field of view, or displays an arrow pointing the direction to the target when it is out of sight (aligned with the screen borders).
- [User Interface](user_interface/user_interface/README.md): On-the-fly generation of a user interface from a .ui file and setting event handlers for widgets.
- [Widget Canvas](user_interface/widget_canvas/README.md): Using the WidgetCanvas class to draw vector-based shapes and text. The canvas supports adding lines, polygons, and text by defining their geometry through vertex positions. Elements are layered by draw order and colored individually.
- [Widget Containers](user_interface/widget_containers/README.md): Demonstration of various types of widget containers (vertical and horizontal box layouts, paned widgets, tabbed interfaces, group boxes, grid-based container, etc.) you can use when creating a User Interface.
- [Widget Dialog](user_interface/widget_dialog/README.md): Creating widget dialog windows and assigning handlers via the C# API.
- [Widget Manipulators](user_interface/widget_manipulators/README.md): This sample demonstrates the use of manipulators. You can lock axes for transformations, and apply transformation in local or world coordinates.
- [Widget Window](user_interface/widget_window/README.md): Creating a basic WidgetWindow using the C# API and handling user interactions (edit line and button press events) through handler function connections.
- [Widgets](user_interface/widgets/README.md): Implementation of various UI widgets, such as sliders, scrolls, buttons, checkboxes, drop-down lists and others.
