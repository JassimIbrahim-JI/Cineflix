$(function () {

    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    $('#ImageUrl').on('change keyup paste', function () {
        const url = $(this).val();
        if (url && isValidUrl(url)) {
            $('#imagePreview').attr('src', url).removeClass('d-none').on('error', function () {
                $(this).addClass('d-none');
            });
        } else {
            $('#imagePreview').addClass('d-none');
        }
    });

    $('#ImageFileInput').on('change', function () {
        const file = this.files[0];
        const preview = $('#imagePreview');

        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                preview.attr('src', e.target.result).removeClass('d-none');
            };
            reader.readAsDataURL(file);
        } else {
            preview.attr('src', '#').addClass('d-none');
        }
    });



    const today = new Date().toISOString().split('T')[0];
    $('#Birthday, #DateOfBirth').attr('max', today).attr('min', '1900-01-01');


    $('#createActorForm, #editActorForm').on('submit', function (e) {
        const name = $('#Name').val().trim();
        if (!name) {
            alert('Actor name is required');
            e.preventDefault();
            return false;
        }

        const dob = new Date($('#Birthday, #DateOfBirth').val());
        const age = new Date().getFullYear() - dob.getFullYear();

        if (age < 0 || age > 120) {
            alert('Please enter a valid date of birth');
            e.preventDefault();
            return false;
        }

        return true;
    });

    // Movie form validation
    $('#movieForm, #editMovieForm').on('submit', function (e) {
        const rating = parseFloat($('#Rating').val());
        if (rating && (rating < 1 || rating > 5)) {
            alert('Rating must be between 1 and 5');
            e.preventDefault();
            return false;
        }

        const duration = parseInt($('#DurationMinutes').val());
        if (duration && duration <= 0) {
            alert('Duration must be greater than 0');
            e.preventDefault();
            return false;
        }

        const price = parseFloat($('#Price').val());
        if (price && price < 0) {
            alert('Price cannot be negative');
            e.preventDefault();
            return false;
        }

        return true;
    });

    const todayMovie = new Date().toISOString().split('T')[0];
    $('#ReleaseDate').attr('min', '1900-01-01').attr('max', todayMovie);

    // Select2 for actors
    $('#actorSelect').select2({
        placeholder: "Select actors...",
        allowClear: true,
        width: '100%'
    });


    let index = $("#actorsTable tbody tr").length;

    $("#addActor").on('click', function () {
        let row = `<tr>
        <td>
            <select name="Actors[${index}].ActorId" class="form-select">
                <option value="">-- Select Actor --</option>
                ${availableActorsOptions()}
            </select>
        </td>
        <td>
            <input name="Actors[${index}].CharacterName" class="form-control" placeholder="e.g., Joker" />
        </td>
        <td>
            <button type="button" class="btn btn-sm btn-danger removeRow">x</button>
        </td>
    </tr>`;

        $("#actorsTable tbody").append(row);
        index++;
    });


    $(document).on('click', '.removeRow', function () {
        $(this).closest('tr').remove();
    });

    function availableActorsOptions() {
        return $("#actorOptions").html(); 
    }


    function isValidUrl(string) {
        try {
            new URL(string);
            return true;
        } catch (_) {
            return false;
        }
    }
});
