// Generated by sprotodump. DO NOT EDIT!
using System;
using Sproto;
using System.Collections.Generic;

public class Protocol : ProtocolBase {
	public static  Protocol Instance = new Protocol();
	private Protocol() {
		Protocol.SetProtocol<bar> (bar.Tag);

		Protocol.SetProtocol<blackhole> (blackhole.Tag);
		Protocol.SetRequest<SprotoType.blackhole.request> (blackhole.Tag);

		Protocol.SetProtocol<foo> (foo.Tag);
		Protocol.SetResponse<SprotoType.foo.response> (foo.Tag);

		Protocol.SetProtocol<foobar> (foobar.Tag);
		Protocol.SetRequest<SprotoType.foobar.request> (foobar.Tag);
		Protocol.SetResponse<SprotoType.foobar.response> (foobar.Tag);

		Protocol.SetProtocol<playopt> (playopt.Tag);
		Protocol.SetRequest<SprotoType.playopt.request> (playopt.Tag);
		Protocol.SetResponse<SprotoType.playopt.response> (playopt.Tag);

	}

	public class bar {
		public const int Tag = 3;
	}

	public class blackhole {
		public const int Tag = 4;
	}

	public class foo {
		public const int Tag = 2;
	}

	public class foobar {
		public const int Tag = 1;
	}

	public class playopt {
		public const int Tag = 1000;
	}

}