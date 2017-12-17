
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

//LibLog violations
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1005:Delegate invocation can be simplified.", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.Logging.LogProvider")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1005:Delegate invocation can be simplified.", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.Logging.LogProviders.DisposableAction")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.Logging.LogProviders.LogMessageFormatter")]

//BoDi violations
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1005:Delegate invocation can be simplified.", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0016:Use 'throw' expression", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainer.ResolutionList")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0016:Use 'throw' expression", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainer.RegistrationKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Embedded external library", Scope = "type", Target = "~T:REstate.IoC.BoDi.ObjectContainerException")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "<Pending>", Scope = "member", Target = "~M:REstate.IoC.BoDi.ObjectContainer.RegistrationKey.Equals(System.Object)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039:Use local function", Justification = "<Pending>", Scope = "member", Target = "~M:REstate.Logging.LoggerExecutionWrapper.Log(REstate.Logging.LogLevel,System.Func{System.String},System.Exception,System.Object[])~System.Boolean")]