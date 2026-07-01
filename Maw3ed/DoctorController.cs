using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using Maw3ed.DAL.DoctorDev.DoctorDtos;
using System;
using System.Threading.Tasks; // إضافة المكتبة

namespace Maw3ed.APIs.DoctorController
{
    [Route("doctor/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorManager _doctorManager;
        public DoctorController(IDoctorManager doctorManager)
        {
            _doctorManager = doctorManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() // تعديل هنا
        {
            var doctors = await _doctorManager.GetAllAsync(); // تعديل هنا
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) // تعديل هنا
        {
            var doctor = await _doctorManager.GetByIdAsync(id); // تعديل هنا

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorCreateDto doctorDto) // تعديل هنا
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _doctorManager.AddAsync(doctorDto); // تعديل هنا

            return Ok(new { message = "Doctor created successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DoctorUpdateDto doctorDto) // تعديل هنا
        {
            if (id != doctorDto.Id)
                return BadRequest("Id mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _doctorManager.UpdateAsync(doctorDto); // تعديل هنا
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) // تعديل هنا
        {
            await _doctorManager.DeleteAsync(id); // تعديل هنا

            return NoContent();
        }
    }
}