// Get references to the necessary elements
//const startButton = document.getElementById('startRecording');
//const stopButton = document.getElementById('stopRecording');
const startRecordingContainer = document.getElementById('startRecordingContainer');
const stopRecordingContainer = document.getElementById('stopRecordingContainer');
// const audioPlayer = document.getElementById('audioPlayer');

// Variables for audio stream and recorder
//let audioStream;
//let mediaRecorder;
//let recordedChunks = [];

//// Handler for the "Start Recording" button click event
//startButton.addEventListener('click', async () => {
//    try {
//        startRecordingContainer.classList.add('d-none');
//        stopRecordingContainer.classList.remove('d-none');


//        // Get the audio stream from the microphone
//        audioStream = await navigator.mediaDevices.getUserMedia({ audio: true });

//        // Create a MediaRecorder instance to record the audio
//        mediaRecorder = new MediaRecorder(audioStream);

//        // Event handler for data available
//        mediaRecorder.addEventListener('dataavailable', (event) => {
//            if (event.data.size > 0) {
//                recordedChunks.push(event.data);
//            }
//        });

//        // Start recording
//        mediaRecorder.start();

//        console.log('Recording started');
//    } catch (error) {
//        console.error('Error starting recording:', error);
//    }
//});

//// Handler for the "Stop Recording" button click event
//stopButton.addEventListener('click', () => {
//    debugger;
//    console.log("Stop fired");
//    if (mediaRecorder && mediaRecorder.state === 'recording') {
//        // Stop recording
//        mediaRecorder.stop();

//        console.log('Recording stopped');
//    }


//    // Event handler for the MediaRecorder's "stop" event
//    mediaRecorder.addEventListener('stop', () => {
//        // Create a Blob with the recorded audio data
//        const audioBlob = new Blob(recordedChunks, { type: 'audio/webm' });

//        // Set the audio element's source to the recorded audio
//        //audioPlayer.src = URL.createObjectURL(audioBlob);

//        $(`#messageList`).append(` <div class="ownMessageImg">
//                                <audio controls>
//                                    <source src="${URL.createObjectURL(audioBlob)}">
//                                </audio>
//                               <span class="time">00:00</span>
//                            </div>`);

//        // Clear the recordedChunks array for future recordings
//        recordedChunks = [];

//        // Release the audio stream resources
//        if (audioStream) {
//            audioStream.getTracks().forEach(track => track.stop());
//            audioStream = null;
//        }
//    });

//    startRecordingContainer.classList.remove('d-none');
//    stopRecordingContainer.classList.add('d-none');
//});


// Global variables
let audioContext;
let mediaRecorder;
let chunks = [];
let canvas;
let canvasContext;
let analyser;

// Function to start audio recording
function startRecording() {

    // Toggle Start and Stop Icon
    startRecordingContainer.classList.add('d-none');
    stopRecordingContainer.classList.remove('d-none');

    navigator.mediaDevices.getUserMedia({
        audio: {
            sampleRate: { ideal: 48000 },
            echoCancellation: true
        }
    })
        .then(stream => {
            // Create an audio context
            audioContext = new AudioContext({ sampleRate: 44100 });

            // Create a media stream source from the user's microphone
            const source = audioContext.createMediaStreamSource(stream);

            // Create an analyser node for audio analysis
            analyser = audioContext.createAnalyser();

            // Connect the nodes
            source.connect(analyser);
            analyser.connect(audioContext.destination);

            // Create a media recorder to record audio
            mediaRecorder = new MediaRecorder(stream);

            // Add an event listener for recording data
            mediaRecorder.addEventListener('dataavailable', e => {
                chunks.push(e.data);
            });

            // Start recording
            mediaRecorder.start();

            // Start visualizing
            visualize();
        })
        .catch(error => {
            console.error('Error accessing microphone:', error);
        });
}

// Function to stop audio recording
function stopRecording() {
    mediaRecorder.stop();

    // Export the recorded audio
    mediaRecorder.addEventListener('stop', () => {
        const audioBlob = new Blob(chunks, { type: 'audio/wav' });
        const audioUrl = URL.createObjectURL(audioBlob);

        // Do something with the recorded audio URL (e.g., play it)
        //const audioElement = document.createElement('audio');
        //audioElement.src = audioUrl;
        //audioElement.controls = true;
        //document.body.appendChild(audioElement);

        $(`#messageList`).append(` <div class="ownMessageImg">
                                <audio controls>
                                    <source src="${audioUrl}">
                                </audio>
                               <span class="time">00:00</span>
                            </div>`);

        // Reset variables
        chunks = [];
        audioContext.close();
        audioContext = null;
        mediaRecorder = null;
    });

    // Toggle Start and Stop Icon
    startRecordingContainer.classList.remove('d-none');
    stopRecordingContainer.classList.add('d-none');

}

// Function to visualize audio in real-time
function visualize() {
    // Get canvas and its context
    canvas = document.getElementById('visualizer');
    canvasContext = canvas.getContext('2d');

    // Set canvas dimensions
    const canvasWidth = canvas.width;
    const canvasHeight = canvas.height;

    // Set up analyser node
    const bufferLength = analyser.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);

    // Clear canvas
    canvasContext.clearRect(0, 0, canvasWidth, canvasHeight);

    // Draw visualizer
    function draw() {
        // Request next frame
        requestAnimationFrame(draw);

        // Get frequency data
        analyser.getByteFrequencyData(dataArray);

        // Clear canvas
        canvasContext.clearRect(0, 0, canvasWidth, canvasHeight);

        // Set bar style
        const barWidth = (canvasWidth / bufferLength) * 2.5;
        let barHeight;
        let x = 0;

        // Draw frequency bars
        for (let i = 0; i < bufferLength; i++) {
            barHeight = dataArray[i];

            canvasContext.fillStyle = 'rgb(' + (barHeight + 100) + ',50,50)';
            canvasContext.fillRect(x, canvasHeight - barHeight / 2, barWidth, barHeight / 2);

            x += barWidth + 1;
        }
    }

    // Start drawing
    draw();
}




