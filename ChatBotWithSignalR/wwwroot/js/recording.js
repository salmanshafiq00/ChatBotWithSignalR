// Get references to the necessary elements
const startRecordingContainer = document.getElementById('startRecordingContainer');
const stopRecordingContainer = document.getElementById('stopRecordingContainer');

// Variables for audio stream and recorder
let audioStream;
let mediaRecorder;
let recordedChunks = [];

// Handler for the "Start Recording" button click event
function startRecording() {
    try {
        startRecordingContainer.classList.add('d-none');
        stopRecordingContainer.classList.remove('d-none');


        // Get the audio stream from the microphone
        audioStream = navigator.mediaDevices.getUserMedia({
            audio: {
                sampleRate: { ideal: 48000 },
                echoCancellation: true
            }
        });

        // Create a MediaRecorder instance to record the audio
        mediaRecorder = new MediaRecorder(audioStream);

        // Event handler for data available
        mediaRecorder.addEventListener('dataavailable', (event) => {
            if (event.data.size > 0) {
                recordedChunks.push(event.data);
            }
        });

        // Start recording
        mediaRecorder.start();

        console.log('Recording started');
    } catch (error) {
        console.error('Error starting recording:', error);
    }
}


// Handler for the "Stop Recording" button click event
function stopRecording() {
    try {
        if (mediaRecorder && mediaRecorder.state === 'recording') {
            // Stop recording
            mediaRecorder.stop();

            // Event handler for the MediaRecorder's "stop" event
            mediaRecorder.addEventListener('stop', () => {
                // Create a Blob with the recorded audio data
                const audioBlob = new Blob(recordedChunks, { type: 'audio/webm' });

                // Set the audio element's source to the recorded audio
                //audioPlayer.src = URL.createObjectURL(audioBlob);

                $(`#messageList`).append(` <div class="ownMessageImg">
                                <audio controls>
                                    <source src="${URL.createObjectURL(audioBlob)}">
                                </audio>
                               <span class="time">00:00</span>
                            </div>`);

                // Clear the recordedChunks array for future recordings
                recordedChunks = [];

                // Release the audio stream resources
                if (audioStream) {
                    audioStream.getTracks().forEach(track => track.stop());
                    audioStream = null;
                }
            });

            console.log('Recording stopped');

            startRecordingContainer.classList.remove('d-none');
            stopRecordingContainer.classList.add('d-none');
        }
    } catch (error) {

    }
}







