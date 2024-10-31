document.addEventListener('DOMContentLoaded', () => {
    const calendarBody = document.getElementById('calendar-body');

    // Generate calendar for a week
    const startDate = new Date(); // Start from the current date
    const timeSlots = ['9:00 AM', '12:00 PM', '3:00 PM']; // Define time slots

    for (let i = 0; i < 7; i++) {
        const currentDate = new Date(startDate);
        currentDate.setDate(currentDate.getDate() + i);

        const row = document.createElement('tr');

        // Add the date cell
        const dateCell = document.createElement('td');
        dateCell.textContent = currentDate.toDateString().slice(4, 10); // Display date (e.g., Oct 10)
        row.appendChild(dateCell);

        // Add time slot cells
        timeSlots.forEach(time => {
            const cell = document.createElement('td');
            cell.textContent = time;
            cell.classList.add('available'); // Default state is "available"

            // Toggle selection when the slot is clicked
            cell.addEventListener('click', () => {
                if (cell.classList.contains('available')) {
                    clearSelectedSlots();
                    cell.classList.remove('available');
                    cell.classList.add('selected'); // Mark slot as "selected"
                } else if (cell.classList.contains('selected')) {
                    cell.classList.remove('selected');
                    cell.classList.add('available'); // Toggle back to "available"
                }
            });

            row.appendChild(cell);
        });

        calendarBody.appendChild(row);
    }

    // Function to clear all selected slots (only one selection allowed)
    function clearSelectedSlots() {
        const selectedSlots = document.querySelectorAll('.selected');
        selectedSlots.forEach(slot => {
            slot.classList.remove('selected');
            slot.classList.add('available');
        });
    }
    // After the `clearSelectedSlots` function
    cell.addEventListener('click', () => {
        if (cell.classList.contains('available')) {
            clearSelectedSlots();
            cell.classList.remove('available');
            cell.classList.add('selected');

            // Set the hidden input value to the selected date and time
            const selectedDate = dateCell.textContent;
            const selectedTime = cell.textContent;
            document.getElementById('appointmentSlot').value = `${selectedDate} ${selectedTime}`;
        } else if (cell.classList.contains('selected')) {
            cell.classList.remove('selected');
            cell.classList.add('available');
            document.getElementById('appointmentSlot').value = ""; // Clear the value if deselected
        }
    });

});
