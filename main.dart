import 'dart:convert';
//import 'dart:html';
import 'package:location/location.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

Coor globalCoor = Coor(18, "", 0.00, 0.00);
void main() => runApp(const MyApp());

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Use Kensnz',
      theme: ThemeData(
        primarySwatch: Colors.deepPurple,
      ),
      home: const HomeWidget(),
    );
  }
}

class HomeWidget extends StatelessWidget {
  const HomeWidget({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
        length: 2,
        child: Scaffold(
          appBar: AppBar(
            title: const Text(
              'Post to Kensnz',
            ),
          ),
          body: const TabBarView(children: <Widget>[
            MyInfo(),
            LocationHomeBody(),
          ]),
          bottomNavigationBar: const TabBar(
            unselectedLabelColor: Colors.blueGrey,
            labelColor: Colors.blueAccent,
            tabs: <Widget>[
              Tab(
                text: ('My Info'),
                icon: Icon(
                  Icons.info,
                  color: Colors.blueGrey,
                ),
              ),
              Tab(
                text: ('Send Location'),
                icon: Icon(
                  Icons.gps_fixed_rounded,
                  color: Colors.amber,
                ),
              )
            ],
          ),
        ));
  }
}

class MyInfo extends StatelessWidget {
  const MyInfo({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      alignment: Alignment.center,
      child: Container(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: const <Widget>[
            Text("Name: Thany Trann"),
            Text("ID: 18"),
          ],
          mainAxisAlignment: MainAxisAlignment.center,
        ),
      ),
    );
  }
}

class PostHomeBody extends StatefulWidget {
  const PostHomeBody({Key? key}) : super(key: key);

  @override
  _PostHomeBodyState createState() => _PostHomeBodyState();
}

class _PostHomeBodyState extends State<PostHomeBody> {
  @override
  Widget build(BuildContext context) {
    return Center(
      child: ElevatedButton(
        child: const Text("Send"),
        onPressed: () {
          sendData();
        },
      ),
    );
  }

  //Post to Web API
  Future<http.Response> addPost(
      int userId, String name, double latitude, double longitude) {
    Map data = {
      'latitude': latitude,
      'longitude': longitude,
      'userId': userId,
      'description': name,
    };
    //https://jsonplaceholder.typicode.com/posts
    //http://developer.kensnz.com/api/addlocdata
    return http.post(Uri.parse("http://developer.kensnz.com/api/addlocdata"),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8'
        },
        body: jsonEncode(data));
  }

  sendData() async {
    http.Response response = await addPost(globalCoor.userId, globalCoor.name,
        globalCoor.latitude, globalCoor.longitude);

    if (response.statusCode == 201) {
      _showMessage(context, "Data Saved\n" + response.body, 30);
    } else {
      _showMessage(context, "Data not able to be saved", 30);
    }
  }

  void _showMessage(BuildContext context, String message, int delay) {
    final scaffold = ScaffoldMessenger.of(context);
    scaffold.showSnackBar(SnackBar(
      content: Text(message),
      duration: Duration(seconds: delay),
      action: SnackBarAction(
        label: "OK",
        onPressed: scaffold.hideCurrentSnackBar,
      ),
    ));
  }
  //Post to Web API End

}

class LocationHomeBody extends StatefulWidget {
  const LocationHomeBody({Key? key}) : super(key: key);

  @override
  _LocationHomeBodyState createState() => _LocationHomeBodyState();
}

class _LocationHomeBodyState extends State<LocationHomeBody> {
  Location location = Location();
  late bool serviceEnabled;
  late PermissionStatus permissionGranted;
  late LocationData locData;
  bool locationFound = false;
  String _coorName = 'unknown';

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: [
        ElevatedButton(
          onPressed: () async {
            serviceEnabled = await location.serviceEnabled();
            if (!serviceEnabled) {
              serviceEnabled = await location.requestService();
              if (serviceEnabled) return;
            }
            permissionGranted = await location.hasPermission();
            if (permissionGranted == PermissionStatus.denied) {
              permissionGranted = await location.requestPermission();
              if (permissionGranted != PermissionStatus.granted) return;
            }
            locData = await location.getLocation();
            setState(() {
              locationFound = true;
              globalCoor.latitude = locData.latitude!;
              globalCoor.longitude = locData.longitude!;
              //if(locationFound)
            });
          },
          child: const Text('Get This Location'),
        ),
        locationFound
            ? Text('Location: (${locData.latitude},${locData.longitude})')
            : Container(),
        const Text('Give a name to this coordinate:'),
        TextField(
          onChanged: (String value) async {
            _coorName = value;
            // ignore: avoid_print
            //print('Username: ' + _coorName);
          },
          decoration: const InputDecoration(
            border: OutlineInputBorder(),
            hintText: 'Give a name to this coordinate',
          ),
        ),
        ElevatedButton(
            onPressed: () async {
              // ignore: avoid_print
              //print('Saving' + _username);
              globalCoor.name = _coorName;
              sendData();
            },
            child: const Text('Send this coordination'))
      ],
    );
  }

  //Post to Web API
  Future<http.Response> addPost(double latitude, double longitude) {
    Map data = {
      'latitude': latitude,
      'longitude': longitude,
      'userid': globalCoor.userId,
      'description': globalCoor.name,
    };
    //https://jsonplaceholder.typicode.com/posts
    //http://developer.kensnz.com/api/addlocdata
    return http.post(Uri.parse("http://developer.kensnz.com/api/addlocdata"),
        headers: <String, String>{
          'Content-Type': 'application/json; charset=UTF-8'
        },
        body: jsonEncode(data));
  }

  sendData() async {
    http.Response response =
        await addPost(globalCoor.latitude, globalCoor.longitude);

    if (response.statusCode == 201) {
      _showMessage(context, "Data Saved\n" + response.body, 30);
    } else {
      _showMessage(context, "Data not able to be saved", 30);
    }
  }

  void _showMessage(BuildContext context, String message, int delay) {
    final scaffold = ScaffoldMessenger.of(context);
    scaffold.showSnackBar(SnackBar(
      content: Text(message),
      duration: Duration(seconds: delay),
      action: SnackBarAction(
        label: "OK",
        onPressed: scaffold.hideCurrentSnackBar,
      ),
    ));
  }
}

class Coor {
  int userId;
  double latitude;
  double longitude;
  String name;
  Coor(this.userId, this.name, this.latitude, this.longitude);
}
