
setlocal

@rem enter this directory
cd /d %~dp0

@rem echo %cd%

set TOOLS_PATH=..\packages\Grpc.Tools.1.4.1\tools\windows_x86

%TOOLS_PATH%\protoc.exe -Iprotos --csharp_out PubSub  protos/pubsub.proto --grpc_out PubSub --plugin=protoc-gen-grpc=%TOOLS_PATH%\grpc_csharp_plugin.exe

endlocal