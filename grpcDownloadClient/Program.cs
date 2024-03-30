using Google.Protobuf;
using Grpc.Net.Client;
using grpcFileTransportDownloadClient;
using FileInfo = grpcFileTransportDownloadClient.FileInfo;

var channel = GrpcChannel.ForAddress("http://localhost:5170");
var client = new FileService.FileServiceClient(channel);

string downloadPath = @"C:\Users\d_zer\RiderProjects\gRPCFileStreaming\grpcDownloadClient\DownloadFiles";

var fileInfo = new FileInfo
{
    FileExtension = ".mp4",
    FileName = "bladeRunner"
};

FileStream fileStream = null;

var request = client.FileDownload(fileInfo);

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

int count = 0;
decimal chunkSize = 0;
while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
{
    if (count++ == 0)
    {
        fileStream =
            new FileStream(
                @$"{downloadPath}\{request.ResponseStream.Current.Info.FileName}{request.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);
        fileStream.SetLength(request.ResponseStream.Current.FileSize);
    }

    var buffer = request.ResponseStream.Current.Buffer.ToByteArray();
    await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);
    
    Console.WriteLine($"{Math.Round(((chunkSize += request.ResponseStream.Current.ReadedByte) * 100)/request.ResponseStream.Current.FileSize)}%");
}

Console.WriteLine("Yüklendi...");
await fileStream.DisposeAsync();
fileStream.Close();