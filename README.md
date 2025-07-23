# BrainSimY: Forging Embodied AI Experiences

**A Neuro-Symbolic Platform for True Understanding**

BrainSimY is a research platform dedicated to a fundamental challenge in artificial intelligence: moving beyond pattern recognition to create systems with genuine understanding. This project extends the pioneering BrainSimIII, leveraging its powerful Universal Knowledge Store (UKS) to serve a new purpose: providing a sophisticated "embodied experience" for Large Language Models (LLMs).

We're bridging the fluid, intuitive capabilities of neural networks with the rigorous, logical framework of a symbolic commonsense engine, creating a new form of hybrid intelligence.

## Core Pillars

### The Universal Knowledge Store (UKS): The Mind's Canvas
- Graph-based knowledge representation with **inheritance with exceptions** (mimicking human cognition)
- **Temporal knowledge** with expiration and **probabilistic reasoning** with confidence scores
- **Conditional logic** and rule-based reasoning that handles nebulous/conflicting information

### The Hybrid C#/Python Architecture: A Bilingual Brain
- C# performance for knowledge operations with Python flexibility for LLM integration
- Seamless communication via Python.NET bridge
- Modular system for easy extension and experimentation

### The LLM-UKS Bridge: The Neuro-Symbolic Heart
- Implements the **Mediated Reasoning Loop** architecture (LLM as intuitive proposer, UKS as logical verifier)
- **Dual confidence scoring** prevents hallucinations
- **Knowledge grounding** through entity linking and disambiguation
- **Dynamic learning** through candidate belief systems

## Our Journey: The LLM Integration Roadmap

### Phase 1: The Foundational Bridge ✅ (Completed)
- [x] Python-LLM bridge with OpenAI/Anthropic integration
- [x] Context management system for UKS knowledge extraction
- [x] Knowledge processing engine for LLM response integration
- [x] Basic query-response cycles with UKS context

### Phase 2: Enhanced Reasoning (In Progress)
- [ ] Collaborative reasoning system
- [ ] Adaptive learning capabilities
- [ ] Confidence scoring and validation systems
- [ ] Temporal reasoning and knowledge decay

### Phase 3: Towards Embodiment
- [ ] Digital avatar creation and persistence
- [ ] Goal-oriented behavior systems
- [ ] Multi-modal knowledge integration
- [ ] Self-modification capabilities

[Detailed roadmap available in LLM_Integration_Roadmap.md](Documentation/LLM_Integration_Roadmap.md)

## How to Experience BrainSimY

### For Researchers & Philosophers
Join us in exploring the nature of consciousness, knowledge, and understanding. We believe intelligence emerges from meaningful interaction between different ways of knowing.

### For Developers
[Getting Started Guide](Documentation/GettingStarted.md) | [Code Structure](Documentation/CodeStructure.md) | [Module System](Documentation/ModuleSystem.md)

### For Collaborators
- **Code**: Contribute modules, optimize performance, extend UKS capabilities
- **Knowledge**: Help encode common sense knowledge, validate candidate beliefs
- **Ideas**: Participate in Future AI Society discussions about the future of AI

## About the UKS: The Foundation of Understanding

The UKS includes a graph of nodes connected by edges. Within the UKS, nodes are called "Things". Things can be related by edges called "Relationships" which consist of a source Thing, a target Thing, and a relationship Type (which is also a Thing). For example: Fido is-a dog would be represented by a single is-a relationship relating Things representing "Fido" and "dog" with the "is-a" Relationship type.

The UKS implements inheritance so that Relationships which add attributes to the dog Thing will also be expressed as attributes of Fido and any other dog. Given that "dogs have 4 legs"; querying Fido will automatically include the fact that Fido has 4 legs even though that information is never explicitly represented. The inheritance process supports exceptions so that adding the information that Tripper, a dog, has 3 legs will override the inheritance process. This combination of inheritance and exceptions is a huge step forward in efficiency similar to the human mind…you don't ever need to store all the attributes of a Thing, only those attributes which make a given Thing unique.

In the same way that Relationships relate multiple Things, "Clauses" relate multiple Relationships. This is important because not all "facts" are either true or false but are dependent on other information. Consider "Fido can play fetch IF the weather is sunny."

The UKS enables BrainSimY to:
- Represent multi-sensory information so that sounds, words, and images can be related
- Represent a real-time mental model of immediate surroundings akin to the mind's similar ability
- Handle nebulous and/or conflicting information
- Store action information so it learns which actions lead to positive outcomes for a current situation
- Update content in real time to handle real-world robotic applications
- Incorporate agent software modules to perform any desired functionality

## More Than Code

This project is an exploration into the nature of consciousness, knowledge, and understanding. We are inspired by the Future AI Society's mission to create AI with common sense—the ability to set goals, build mental models, and understand physics like a child.

With the UKS, this project is leapfrogging other AI technologies which are unable to represent the information needed for the understanding which underpins Common Sense. We're not just building another AI system—we're creating a new paradigm for embodied artificial intelligence.

[Future AI Society](https://futureaisociety.org) | [Project Documentation](Documentation/) | [LLM Bridge Prototype](LLM_Bridge_Prototype/)

---

*Thanks for your interest in pushing the boundaries of what AI can become!* 

