# C++ Code Samples


## Animation Characters

- [Bones: Constraints [Animation Graph]](animation_characters/bones_constraints/README.md): Applying bone rotation constraints and observing how they affect inverse kinematics.
- [Bones: Foot Placement [Animation Graph]](animation_characters/bones_foot_placement/README.md): Adjusting the feet of a character to the height and the slope of an uneven surface using inverse kinematics and raycasting.
- [Bones: Inverse Kinematics [Animation Graph]](animation_characters/bones_inverse_kinematics/README.md): Controlling bones with inverse kinematics.
- [Bones: Look At Chains [Animation Graph]](animation_characters/bones_look_at_chains/README.md): Using LookAt chains to aim the bones of a character at a target.
- [Bones: Masks](animation_characters/bones_masks/README.md): Using masks to assign selective logic to different bones.
- [Bones: Retargeting [Animation Graph]](animation_characters/bones_retargeting/README.md): Using the same animation on skeletons with different proportions.
- [Bones: Root Motion [Animation Graph]](animation_characters/bones_root_motion/README.md): Moving an object by the offset of the root bone of its animation.
- [Bones: Sandbox](animation_characters/bones_sandbox/README.md): An interactive sandbox for setting up IK chains, LookAt chains, and bone rotation constraints at runtime.
- [Bones: State Machine [Animation Graph]](animation_characters/bones_state_machine/README.md): Controlling character animation with state machines and blend spaces built in an animation graph.

## Animation Generic

- [Animation Layers Playback](animation_generic/animation_layers_playback/README.md): Demonstration of animation sequences played in order or simultaneously.
- [Bones: Rotation [Animation Graph]](animation_generic/bones_rotation/README.md): Blending skeletal animations in an animation graph and rotating a bone programmatically.
- [Curve2D Animation](animation_generic/curve2d_animation/README.md): Real-time animation of transformations and material parameters using Curve2D for flexible, non-linear motion.
- [Global Engine Parameters Animation](animation_generic/global_engine_parameters_animation/README.md): Animating global Engine parameters, such as physics gravity and render background color, using singleton channels.
- [Material Parameters Animation](animation_generic/material_parameters_animation/README.md): Changing material parameters at runtime.
- [Node Parameters Animation](animation_generic/node_parameters_animation/README.md): Animating position, rotation, and scale of a node using node binds and different types of animation channels.
- [Physics-Based Animation](animation_generic/physics_based_animation/README.md): Physics-based animation of movements using easing functions and spring simulation.
- [Property Animation](animation_generic/property_animation/README.md): Animating a parameter of a property assigned to a node and using the animated value to drive the logic of the sample.
- [Tracker: Playback](animation_generic/tracker_playback/README.md): Using Tracker to animate the position, rotation, and scale of an object.
- [Widget Animation](animation_generic/widget_animation/README.md): Animating widget position, font size, and color using runtime binds.

## App Logic

- [Advanced Event Connection Patterns](app_logic/advanced_event_connection_patterns/README.md): Advanced ways of subscribing to events in UNIGINE: using extra arguments, discarding parameters, and storing connection handles for disconnection.
- [Component Parameters In Editor](app_logic/component_parameters_in_editor/README.md): Demonstration of component parameter types and configuration options.
- [Component System Example](app_logic/component_system_example/README.md): Demonstration of UNIGINE's C++ component-based architecture using custom gameplay components with dynamic object creation and interaction.
- [Console Interaction](app_logic/console_interaction/README.md): Interacting with the Engine's built-in console and adding custom console commands and variables via API using the Console and ConsoleVariable classes.
- [Custom Stream](app_logic/custom_stream/README.md): Creating a custom stream class by inheriting from StreamBase and using it for reading from and writing to files.
- [Euler Angle Composition And Decomposition](app_logic/euler_angle_composition_and_decomposition/README.md): Composing and decomposing object rotation using Euler angles in different axis sequences.
- [Event Connection Patterns](app_logic/event_connection_patterns/README.md): Demonstration of four different patterns of subscribing to UNIGINE's Events via the C++ API, highlighting how event handler lifetime and management can vary depending on the approach.
- [File Operations](app_logic/file_operations/README.md): Demonstration of basic file I/O operations.
- [File System External Package](app_logic/filesystem_external_package/README.md): Demonstration of working with external package files via the Package class.
- [File System Mount Points](app_logic/filesystem_mount_points/README.md): Creating and using mount points in the file system for accessing external folders and package files (e.g., *.zip, *.ung).
- [Inverse FPS Usage](app_logic/inverse_fps_usage/README.md): Using Game::getIFps() to implement movement logic independent of the frame rate.
- [JSON](app_logic/json/README.md): Generation of a structured JSON document containing objects, arrays, and various data types such as strings, numbers, booleans, and null values, followed by traversal and pretty-printed output.
- [Materials And Properties Enumeration](app_logic/materials_and_properties_enumeration/README.md): Working with Property Manager and Material Manager to access all materials and properties in the project via API.
- [Type Safe Callbacks](app_logic/type_safe_callbacks/README.md): Using the CallbackBase class to wrap and call functions and class methods with various numbers of arguments.
- [XML](app_logic/xml/README.md): Creating and manipulating an XML document using the Xml class.

## Input Handling

- [Gamepad](input_handling/gamepad/README.md): This sample demostrates the simple usage of Gamepad input.
- [Joystick](input_handling/joystick/README.md): This sample demonstrates how to add advanced joystick input handling, supporting multiple controllers with real-time axis/button monitoring and force feedback effects.
- [Keyboard And Mouse](input_handling/keyboard_and_mouse/README.md): This sample demonstrates how to add monitoring of keyboard and mouse input, tracking key states, mouse movements, wheel events, and cursor positions across different coordinate systems. It displays real-time input data including key presses, mouse deltas, and text input.
- [Touch](input_handling/touch/README.md): This sample demonstrates how to add multi-touch input from the touchscreen, visualizing finger positions with dynamic circles and displaying real-time coordinates to the project.

## Multi Threading Performance Optimization

- [Asynchronous Meshes And Textures Loading](multi_threading_performance_optimization/asynchronous_meshes_and_textures_loading/README.md): Loading meshes and textures in a separate thread using the AsyncQueue class.
- [Asynchronous Nodes Loading Stress-Test](multi_threading_performance_optimization/asynchronous_nodes_loading_stress_test/README.md): Asynchronous node loading via AsyncQueue with main-thread spatial integration.
- [Asynchronous Tasks Scheduler Configuration](multi_threading_performance_optimization/asynchronous_tasks_scheduler_configuration/README.md): Managing tasks via AsyncQueue class with dirrefent thread types, parallel execution and frame control.
- [CPU Shader Usage](multi_threading_performance_optimization/cpu_shader_usage/README.md): Multi-threaded update of multiple ObjectMeshCluster instances on the CPU side using the CPUShader class.
- [Custom Threads](multi_threading_performance_optimization/custom_threads/README.md): Creating and running custom threads using the Unigine::Thread class.
- [Microprofiler Custom Counters](multi_threading_performance_optimization/microprofiler_custom_counters/README.md): Using Microprofile, an advanced CPU/GPU profiler, to track performance and estimate the time spent on different sections of code.
- [Multiple Async Raycast Requests](multi_threading_performance_optimization/multiple_async_raycast_requests/README.md): Launching and managing a large number of asynchronous ray-based intersection queries simultaneously.
- [Single Async Raycast Request](multi_threading_performance_optimization/single_async_raycast_request/README.md): Performing a single asynchronous intersection query based on the user's mouse cursor position in the scene.

## Navigation

- [Experimental Navigation Mesh](navigation/experimental_navigation_mesh/README.md): Configuring pathfinding between two points around obstacles using Experimental Navigation Mesh.
- [Experimental Navigation Mesh Character](navigation/experimental_navigation_mesh_character/README.md): Moving an animated character over an Experimental Navigation Mesh with the root motion of the animation graph.
- [Experimental Navigation Mesh Conversion](navigation/experimental_navigation_mesh_conversion/README.md): Converting navigation data between the classic Navigation Mesh and the Experimental Navigation Mesh.
- [Experimental Navigation Mesh Crowd](navigation/experimental_navigation_mesh_crowd/README.md): Moving a crowd of agents over an Experimental Navigation Mesh with collision avoidance.
- [Experimental Navigation Mesh Demo](navigation/experimental_navigation_mesh_demo/README.md): Configuring pathfinding to multiple targets on a plane with obstacles using Experimental Navigation Mesh.
- [Experimental Navigation Mesh Queries](navigation/experimental_navigation_mesh_queries/README.md): Performing spatial queries on an Experimental Navigation Mesh: nearest and reachable points, random points, boundaries, and polygons in an area.
- [Experimental Navigation Mesh Terrain](navigation/experimental_navigation_mesh_terrain/README.md): Navigating a large terrain using two levels of detail: a coarse Experimental Navigation Mesh for the whole world and a detailed one streamed and baked around the agents at runtime.
- [Navigation Mesh](navigation/navigation_mesh/README.md): Configuring pathfinding between two points with obstacles using Navigation Mesh.
- [Navigation Mesh Demo](navigation/navigation_mesh_demo/README.md): Configuring pathfinding to multiple targets on a plane with obstacles using Navigation Mesh.
- [Navigation Sectors](navigation/navigation_sectors/README.md): Configuring pathfinding between two points with obstacles using Navigation Sectors.
- [Navigation Sectors Demo](navigation/navigation_sectors_demo/README.md): Configuring pathfinding to multiple targets in a cube with obstacles using Navigation Sector.

## Network

- [HTTP Image request](network/http_image_request/README.md): This sample shows how to implement an asynchronous HTTP request to a REST API to download image files and apply them to scene objects at runtime.
- [HTTP Request Handling](network/http_request_handling/README.md): Implementing asynchronous HTTP GET requests to external REST API and displaying the retrieved data in the user interface.
- [TCP Sockets](network/tcp_sockets/README.md): Establishing and managing TCP socket connections between a server and multiple clients each represented by a UNIGINE-application. Clients can connect to the server, exchange text messages via the Console, and receive camera transform updates from the server.
- [UDP Sockets](network/udp_sockets/README.md): Using the sockets API to send and receive UDP messages in the network between two peers each represented by a UNIGINE-application.

## Nodes

- [Cluster](nodes/cluster/README.md): Dynamic manipulation of ObjectMeshCluster in UNIGINE, showcasing how to add/remove mesh instances at runtime through user interaction.
- [Lights](nodes/lights/README.md): This sample demonstrates how to create light sources (World Light, Projected Light, Omni Light) and modify their parameters at runtime.
- [Node Extern](nodes/node_extern/README.md): Adding custom nodes created via API to the world by using NodeExtern. Implementation of bound box visualization and runtime node configuration while maintaining Engine integration.
- [Object Extern](nodes/object_extern/README.md): Adding custom objects created via API to the world by using ObjectExtern. Implementation of custom rendering, physics, and properties while maintaining full Engine integration.
- [Player Types](nodes/players_types/README.md): Creation and configuration of the four Players available (PlayerDummy, PlayerPersecutor, PlayerSpectator, PlayerActor).
- [Spline Graph](nodes/spline_graph/README.md): Creation and visualization of a spline graph (SplineGraph), and moving an object along it.
- [Water Surface Parameters Fetch](nodes/water_surface_parameters_fetch/README.md): Demonstration of how various parameters influence the accuracy of fetch and intersection operations on the Global Water object across different Beaufort levels.
- [Water Waves Customization Gerstner](nodes/water_waves_customization_gerstner/README.md): This sample demonstrates how to control the wave spectrum of Global Water in Manual mode via API
- [Water Waves Generation Field Height](nodes/water_waves_generation_field_height/README.md): Creation of a dynamic Field Height with dynamic sine function texture using the C++ API.
- [World Spline Graph](nodes/world_spline_graph/README.md): Generating spline-based geometry by creating a WorldSplineGraph from an *.spl file and applying *.node geometry in stretching mode for placing objects along splines.

## Physics

- [Body Events](physics/body_events/README.md): Demonstrating the usage of Frozen, Position, and ContactEnter events of the Body class via the C++ API.
- [Body Fracture Explosion](physics/body_fracture_explosion/README.md): Simulating a radial explosion that triggers BodyFracture object to crack and applies forces to its pieces.
- [Body Fracture Falling Spheres](physics/body_fracture_falling_spheres/README.md): Continuously falling objects fracturing upon collision implemented using the BodyFracture class.
- [Body Fracture Shooting Gallery](physics/body_fracture_shooting_gallery/README.md): Implementation of a basic physics-driven shooting gallery using Fracture Body.
- [Joint Events](physics/joint_events/README.md): Demonstrating the usage of the Broken event of the Joint class.
- [Physics Movement](physics/physics_movement/README.md): Simple logic of moving an object using physical methods (by force or by impulse).
- [Update Physics](physics/update_physics/README.md): Demonstration of the difference between implementation of physics-driven movement within the update() and updatePhysics() methods.

## Player Controllers

- [Camera Zoom](player_controllers/camera_zoom/README.md): Creating interactive camera system with adjustable zoom and focus on selectable scene targets.
- [First-Person Controller](player_controllers/first_person_controller/README.md): Implementation of a first-person character controller with an advanced movement system and collision detection.
- [Observer Controller](player_controllers/observer_controller/README.md): Implementation of a free-flying camera similar to the one used in the UnigineEditor (with zooming, panning, focusing and speed control).
- [Orbit Camera Controller](player_controllers/orbit_camera_controller/README.md): Creating an orbital camera rotating around a target.
- [Persecutor Controller](player_controllers/persecutor_controller/README.md): Creating custom target-following camera using PlayerDummy, replicating PlayerPersecutor logic with adjustable offset, collision handling, and multiple follow modes.
- [Spectator Controller](player_controllers/spectator_controller/README.md): Implementation of a customizable first-person spectator camera with configurable movement and physical collision detection.
- [Top-Down Controller](player_controllers/top_down_controller/README.md): Implementation of some elements of a top-down strategy such as selecting one or multiple units, panning the view, turning the camera and smoothly focusing it on the current selection.
- [Two-Point Perspective](player_controllers/two_point_perspective/README.md): Simulating a two-point perspective projection using a lens shift technique implemented via a secondary Dummy Player.

## Procedural Generation Placement

- [Clutter-To-Cluster Converter](procedural_generation_placement/clutter_to_cluster_converter/README.md): Dynamic generation of a Mesh Clutter and its conversion into a performance-optimized Mesh Cluster at runtime.
- [Grid-Based Node Spawning](procedural_generation_placement/grid_based_node_spawning/README.md): Spawning nodes across a grid.
- [Procedural Mesh Generation](procedural_generation_placement/procedural_mesh_generation/README.md): Demonstrating runtime procedural mesh generation, along with visualization of how different procedural modes impact memory usage during creation and rendering.
- [Procedural Mesh Modification](procedural_generation_placement/procedural_mesh_modification/README.md): Demonstrating runtime procedural mesh modification, along with visualization of how different procedural modes impact streamiing and memory usage during creation and rendering.
- [Procedural Mesh Updates Mesh Clusters](procedural_generation_placement/procedural_mesh_updates_mesh_clusters/README.md): A minimal example that shows how to generate and apply a procedural mesh at runtime using the correct update sequence.
- [Procedural Spline Mesh Generation](procedural_generation_placement/procedural_spline_mesh_generation/README.md): Procedural generation of spline-based meshes from world points with real-time parameter control.
- [Real-Time Mesh Editing Marching Cubes](procedural_generation_placement/real_time_mesh_editing_marching_cubes/README.md): Simulation of the ground digging process based on mesh geometry modification.
- [Sensor Zone Visualizer](procedural_generation_placement/sensor_zone_visualizer/README.md): Procedural frustum mesh generator for visualizing sensor fields of view with live controls for angles, distance, and color.
- [Timer-Based Node Spawning](procedural_generation_placement/timer_based_node_spawning/README.md): Implementing a simple node spawner that creates nodes at a set spawn frequency.

## Rendering

- [CAD-Like View](rendering/cad_like_view/README.md): CAD-Like View implementation with multiple synced viewports for top, side, front, and perspective views.
- [Camera To Texture](rendering/camera_to_texture/README.md): Capturing a camera's output in real time and projecting it onto a material's albedo texture using Viewport::renderTexture2D().
- [Compute Shader](rendering/compute_shader/README.md): Implementing a GPU-based particle system with compute shaders.
- [Compute Shader Image](rendering/compute_shader_image/README.md): Creating a compute shader that processes a read-write texture on GPU (without CPU).
- [FFP Depth-Tested Line Rendering](rendering/ffp_depth_tested_line_rendering/README.md): Rendering custom visual elements (lines) using the FFP with depth testing enabled.
- [FFP Triangle Fan Rendering](rendering/ffp_triangle_fan_rendering/README.md): Using FFP functionality to draw simple 2D shapes over the rendered image without any additional shaders.
- [Gbuffer Read](rendering/gbuffer_read/README.md): Accessing G-buffer textures at different stages of the rendering process by configuring a custom Viewport and intercepting its output at the G-buffer rendering stage.
- [Gbuffer Write](rendering/gbuffer_write/README.md): Modification G-buffer textures at different stages of the rendering process by injecting a custom material at the end of the G-buffer pass.
- [Gui To Texture](rendering/gui_to_texture/README.md): Rendering GUI elements into a texture using Gui::render() and applying the resulting texture to materials.
- [Mesh To Mask Texture](rendering/mesh_to_mask_texture/README.md): Rendering a mesh into a texture using a custom material for mask generation.
- [Node To Texture](rendering/node_to_texture/README.md): Rendering a node into a texture using Viewport::renderNodeTexture2D(), and setting this texture as material's albedo texture
- [Procedural 3D Volume Texture Generation](rendering/procedural_3d_volume_texture_generation/README.md): Procedurally generate 3D image data and use it as a density texture for a volume-based material in real time.
- [Render Target](rendering/render_target/README.md): Rendering paint splashes to a texture using the RenderTarget class.
- [Screenshot](rendering/screenshot/README.md): Simple demonstration of how to make a screenshot by grabbing the final image from the rendering sequence.
- [Split-Screen Texture](rendering/split_screen_texture/README.md): Rendering views from two cameras into textures and implementing split-screen mode.
- [Structured Buffer](rendering/structured_buffer/README.md): Creating a DXT compression by using structured buffers
- [Textures](rendering/textures/README.md): Updating mesh albedo texture using procedurally generated image data.
- [Visualizer](rendering/visualizer/README.md): Demonstration of the full range of features provided by the Visualizer class for visual debugging.
- [Weapon Clipping](rendering/weapon_clipping/README.md): Rendering the weapon from a second camera into a texture to avoid clipping issues.

## Scene Management

- [Bounding Volume Object Detection](scene_management/bounding_volume_object_detection/README.md): Finding intersections between certain volumes (frustum, sphere, and box) and the bounding boxes of nodes.
- [Control Elements](scene_management/control_elements/README.md): Demonstration of various types of interactable buttons and levers.
- [Create And Modify Objects](scene_management/create_and_modify_objects/README.md): Basics of working with Objects: creation and modification via API.
- [Create Mesh Primitives](scene_management/create_mesh_primitives/README.md): Creating parametric 3D primitives at runtime.
- [Day-Night Cycle](scene_management/day_night_cycle/README.md): Implementation of an automated day-night system with light and material switching.
- [Move By Trajectory](scene_management/move_by_trajectory/README.md): Three types of movement along a predefined path: linear interpolation, spline interpolation, and a trajectory loaded from a *.path file.
- [Node Movement](scene_management/node_movement/README.md): Implementing object movement and rotation in 3D space using three different methods.
- [Node State Save-Restore](scene_management/node_state_save_restore/README.md): This sample demonstrates how to save and restore the state of an arbitrary Node, which can be used to implement things like Scene serialization, Undo/Redo systems, or Game saves.
- [Nodes And Widgets Lifetime Control](scene_management/nodes_and_widgets_lifetime_control/README.md): Implementation of configurable node and widget lifetimes with persistence across different worlds.
- [Raycast Detection And Bitmasking](scene_management/raycast_detection_and_bitmasking/README.md): Using raycasting for selective (mask-based) detection of intersections.
- [Raycast From Mouse Position](scene_management/raycast_from_mouse_position/README.md): Finding an intersection between a ray cast from the camera through the mouse cursor and geometry.
- [Trigger System Examples](scene_management/trigger_system_examples/README.md): Implementation of various types of triggers.

## Simulation

- [Speed Boat](simulation/speed_boat/README.md): Simulating dynamic wake foam behind a moving boat using a combination of Orthographic Decals and Particle Systems.

## Sounds

- [3D Sound](sounds/3d_sound/README.md): Playing a sound source via code.
- [Ambient Sound](sounds/ambient_sound/README.md): Playing ambient sounds via code.
- [FMOD Core](sounds/fmod_core/README.md): FMOD Core integration for real-time playback of 2D and 3D sounds with timeline control and DSP effects. NOTICE: Additional libraries are required, run the sample for details.
- [FMOD Studio](sounds/fmod_studio/README.md): FMOD Studio integration with real-time interactive audio effects, spatialization, and Doppler effect simulation. NOTICE: Additional libraries are required, run the sample for details.
- [Reverberation Zone](sounds/reverberation_zone/README.md): Setting a reverberation zone and its parameters via code.

## Terrain Modification Usage

- [Asynchronous Terrain Albedo Height Brushes](terrain_modification_usage/asynchronous_terrain_albedo_height_brushes/README.md): This sample demonstrates real-time terrain modification through direct manipulation of Landscape Layer Maps.
- [Asynchronous Terrain Data Fetch](terrain_modification_usage/asynchronous_terrain_data_fetch/README.md): This sample demonstrates how to retrieve detailed information from a LandscapeTerrain object on its surface using LandscapeFetch.
- [Asynchronous Terrain Mask Brushes](terrain_modification_usage/asynchronous_terrain_mask_brushes/README.md): This sample demonstrates real-time painting of Landscape Layer Map mask data using a customizable brush system.
- [Creating Detail Layers](terrain_modification_usage/creating_detail_layers/README.md): Adding and managing detail layers for a LandscapeTerrain object using the getDetailMask() and addDetail() methods.
- [Generating Mesh From Terrain](terrain_modification_usage/generating_mesh_from_terrain/README.md): This sample demonstrates how to generate a procedural mesh (ObjectMeshDynamic) that represents a selected region of the Landscape Terrain.
- [Generating Terrain From Textures](terrain_modification_usage/generating_terrain_from_textures/README.md): This sample demonstrates how to dynamically generate a Landscape Layer Map using tiled albedo, height, and mask textures.
- [Height Slicing](terrain_modification_usage/height_slicing/README.md): This sample demonstrates a hybrid approach to terrain editing using both non-destructive and destructive Landscape Terrain modification techniques.
- [Polygon-Based Procedural Modifications](terrain_modification_usage/polygon_based_procedural_modifications/README.md): Procedural polygon objects generation based on world points for landscape editing and object placement.
- [Real-Time Excavation](terrain_modification_usage/real_time_excavation/README.md): This sample demonstrates destructive, real-time modification of a Landscape Layer Map using a 3D object (e.g., a tractor grader) as an excavation tool.
- [Real-Time Terrain Rut Deformation](terrain_modification_usage/real_time_terrain_rut_deformation/README.md): Non-destructive runtime modification of the Landscape Terrain by dynamically spawning multiple Landscape Layer Maps beneath moving objects to create realistic track imprints on the terrain.

## Unigine Script Interop

- [Usc Arrays](unigine_script_interop/usc_arrays/README.md): Integration between C++ and UnigineScript by registering external C++ functions that manipulate UnigineScript array types.
- [Usc Callbacks](unigine_script_interop/usc_callbacks/README.md): Calling UnigineScript functions from C++ code via callbacks.
- [Usc Classes](unigine_script_interop/usc_classes/README.md): Exporting classes from C++ side to UnigineScript.
- [Usc Functions](unigine_script_interop/usc_functions/README.md): This sample demonstrates how to export functions from C++ side to UnigineScript. It includes examples of exporting regular functions, handling multiple data types, and registering class members for a singleton-like object.
- [Usc Inheritance](unigine_script_interop/usc_inheritance/README.md): Working with UnigineScript containers via C++ API.
- [Usc Stack](unigine_script_interop/usc_stack/README.md): Using a stack implemented via C++ in UnigineScript.
- [Usc Structures](unigine_script_interop/usc_structures/README.md): Exposing C++ structs to UnigineScript using the Interpreter class.
- [Usc Transfer](unigine_script_interop/usc_transfer/README.md): Transfering complex data between UnigineScript and C++ using the Variable class and the TypeToVariable utility.
- [Usc Types](unigine_script_interop/usc_types/README.md): Enabling type conversion between custom C++ types and UnigineScript using the Variable class.
- [Usc Variable](unigine_script_interop/usc_variable/README.md): Working with different variable types in UnigineScript using the Variable class from C++ code.

## User Interface

- [Fonts](user_interface/fonts/README.md): Multi-script text rendering (English, Russian, Arabic, Thai, Chinese, Hindi) across GUI widgets, 3D ObjectText, and world-space ObjectGui, with configurable text direction, cursor mode, and font size.
- [Measurements](user_interface/measurements/README.md): Dynamically measuring and visualizing the distance from a movable node to surrounding target nodes.
- [Object Frame](user_interface/object_frame/README.md): Using the WidgetCanvas class to render custom frames around objects in the viewport and save frame metadata as JSON.
- [Object Text](user_interface/object_text/README.md): Demonstration of 3D text objects with adjustable parameters, animated colors, and rich formatting using the ObjectText class.
- [Target Marker](user_interface/target_marker/README.md): Implementation of a marker that always highlights the target when it is within the field of view, or displays an arrow pointing the direction to the target when it is out of sight (aligned with the screen borders).
- [User Interface](user_interface/user_interface/README.md): On-the-fly generation of a user interface from a .ui file and setting event handlers for widgets.
- [Widget Canvas](user_interface/widget_canvas/README.md): Using the WidgetCanvas class to draw vector-based shapes and text. The canvas supports adding lines, polygons, and text by defining their geometry through vertex positions. Elements are layered by draw order and colored individually.
- [Widget Dialogs](user_interface/widget_dialogs/README.md): Creating widget dialog windows and assigning handlers via the C++ API
- [Widget Extern](user_interface/widget_extern/README.md): Defining a custom widget via the C++ API using WidgetExternBase, with full control over layout, rendering, and interaction.
- [Widget Manipulators](user_interface/widget_manipulators/README.md): This sample demonstrates the use of manipulators. You can lock axes for transformations, and apply transformation in local or world coordinates.
- [Widget Window](user_interface/widget_window/README.md): Creating a basic WidgetWindow using the C++ API and handling user interactions (edit line and button press events) through handler function connections.
- [Widgets](user_interface/widgets/README.md): Implementation of various UI widgets, such as sliders, scrolls, buttons, checkboxes, drop-down lists and others.

## Utils

- [Intersection](utils/intersection/README.md)
- [Navigation](utils/navigation/README.md)
- [Network](utils/network/README.md)
