/* Copyright (C) 2005-2026, UNIGINE. All rights reserved.
*
* This file is a part of the UNIGINE 2 SDK.
*
* Your use and / or redistribution of this software in source and / or
* binary form, with or without modification, is subject to: (i) your
* ongoing acceptance of and compliance with the terms and conditions of
* the UNIGINE License Agreement; and (ii) your inclusion of this notice
* in any version of this software that you use or redistribute.
* A copy of the UNIGINE License Agreement is available by contacting
* UNIGINE. at http://unigine.com/
*/

#pragma once

#include "UnigineNodes.h"
#include "UnigineImage.h"
#include "UnigineString.h"
#include "UnigineObjects.h"
#include "UnigineEngine.h"
#include "UnigineGame.h"

namespace Unigine
{

namespace Utils
{
	UNIGINE_API NodePtr loadObjectMesh(const char *name);
	UNIGINE_API void convertNodesToMesh(MeshPtr &out, const Vector<NodePtr> &nodes);
	UNIGINE_API bool convertHeightsToNormals(ImagePtr &out, const ImagePtr &heights, float step_size = 1.0f);
	UNIGINE_API StringStack<> date(const char *format);
	UNIGINE_API StringStack<> date(const char *format, long long time);

	UNIGINE_API bool makeNodeCurved(NodePtr &node, ObjectMeshStatic::PROCEDURAL_MODE procedural_mode, bool force);
	UNIGINE_API bool makeNodeFlat(NodePtr &node, ObjectMeshStatic::PROCEDURAL_MODE procedural_mode, bool force);

	UNIGINE_API MeshPtr makeMeshCurved(const NodePtr &node);
	UNIGINE_API MeshPtr makeMeshFlat(const NodePtr &node);

	UNIGINE_API const char *getCommandArg(const char *arg_name, const char *default_value = "");
	UNIGINE_API int getCommandArg(const char *arg_name, int default_value = 0);
	UNIGINE_API float getCommandArg(const char *arg_name, float default_value = 0);
	UNIGINE_API bool isCommandArg(const char *arg_name);

	UNIGINE_API bool checkPlugin(const char *plugin_name, bool force_load = false);

	template <class T>
	void collectNodesByType(const Unigine::NodePtr &node, Unigine::Vector<Unigine::Ptr<T>> &nodes)
	{
		using namespace Unigine;
		if (!node || node.isDeleted())
			return;

		if (auto type_casted_node = checked_ptr_cast<T>(node))
			nodes.append(type_casted_node);

		if (node->getType() == Unigine::Node::NODE_REFERENCE)
			collectNodesByType(static_ptr_cast<Unigine::NodeReference>(node)->getReference(), nodes);

		for (int i = 0; i < node->getNumChildren(); i++)
		{
			auto child = node->getChild(i);
			collectNodesByType(child, nodes);
		}
	}

	template <class F>
	UNIGINE_INLINE void recursiveCall_impl(const Unigine::NodePtr &node, F &func)
	{
		if (!node)
			return;

		func(node);

		if (node->getType() == Unigine::Node::NODE_REFERENCE)
		{
			auto ref = Unigine::checked_ptr_cast<Unigine::NodeReference>(node);
			if (ref && ref->getReference())
				recursiveCall_impl(ref->getReference(), func);
		}

		for (int i = 0; i < node->getNumChildren(); i++)
			recursiveCall_impl(node->getChild(i), func);
	}

	template <class F>
	UNIGINE_INLINE void recursiveCall(const Unigine::NodePtr &node, F &&func)
	{
		auto f = std::forward<F>(func);
		recursiveCall_impl(node, f);
	}

	//---------------------------------
	// Execution

	template<typename L, typename std::enable_if<std::is_class<L>::value, bool>::type = false>
	void runLater(int skip_frames, const L &lambda)
	{
		struct State { int frame; Unigine::EventConnection conn; };
		auto *s = new State{skip_frames};
		Unigine::Engine::get()->getEventBeginWorldUpdate().connect(s->conn, [lambda, s]() {
			if (--s->frame < 0)
			{
				lambda();
				delete s;
			}
		});
	}

	template<typename L, typename std::enable_if<std::is_class<L>::value, bool>::type = false>
	void runLater(float skip_seconds, const L &lambda)
	{
		struct State { float timer; Unigine::EventConnection conn; };
		auto *s = new State{skip_seconds};
		Unigine::Engine::get()->getEventBeginWorldUpdate().connect(s->conn, [lambda, s]() {
			if ((s->timer -= Unigine::Game::getIFps()) < 0)
			{
				lambda();
				delete s;
			}
		});
	}

	template <typename L, typename std::enable_if<std::is_class<L>::value, bool>::type = false>
	void runAnimation(float duration, const L &lambda)
	{
		duration = Unigine::Math::max(duration, Unigine::Math::Consts::EPS);
		struct State { float max_time; float timer; Unigine::EventConnection conn; };
		auto *s = new State{duration, duration};
		Unigine::Engine::get()->getEventBeginWorldUpdate().connect(s->conn, [lambda, s]()
		{
			if (!Unigine::Game::isEnabled())
				return;
			s->timer -= Unigine::Game::getIFps();
			float v = 1.0f - Unigine::Math::saturate(s->timer / s->max_time);
			lambda(v);
			if (s->timer < 0.0f)
				delete s;
		});
	}

}

} // namespace
